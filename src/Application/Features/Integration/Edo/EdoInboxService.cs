using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using SharedKernel.Constants;
using SharedKernel.Results;
using System.Globalization;
using ApplicationSigningSession = Application.Abstractions.Integration.Edo.EdoDocumentSigningSession;

namespace Application.Features.Integration.Edo;

public sealed class EdoInboxService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoDocumentStore documentStore,
    IEdoDocumentSigningSessionStore signingSessionStore,
    IEdoIdempotencyService idempotencyService,
    IEdoReconciliationService reconciliationService,
    IAuditLogService auditLogService,
    ILogger<EdoInboxService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoInboxService
{
    private const string InboxSyncOperation = "INBOX_SYNC";
    private const string RejectOperation = "INBOX_REJECT";
    private const string DocumentTable = "edo_document";
    private const string SigningSessionTable = "edo_document_signing_session";
    private static readonly TimeSpan SigningSessionLifetime = TimeSpan.FromMinutes(5);

    public async Task<EdoInboxListDto> ListInboxAsync(EdoInboxQueryDto request, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.ListInbox);

        if (request.Page < 1 || request.PageSize is < 1 or > 100)
            throw new InvalidOperationException("Inbox page must be at least 1 and page size must be between 1 and 100.");

        var providerResult = await provider.ListInboxAsync(request, ct);
        var items = new List<EdoDocumentDto>(providerResult.Items.Count);
        await ExecuteInTransactionAsync("PersistInboxSync", async () =>
        {
            foreach (var providerDocument in providerResult.Items)
            {
                var localDocument = await UpsertInboxDocumentAsync(organizationId, provider.Code, providerDocument, ct);
                items.Add(MapDocument(localDocument, providerDocument));
            }

            return Result.Success();
        }, ct);

        return new EdoInboxListDto
        {
            Items = items,
            Page = providerResult.Page,
            PageSize = providerResult.PageSize,
            TotalCount = providerResult.TotalCount
        };
    }

    public async Task<EdoInboxRejectDto> RejectAsync(
        long id,
        EdoInboxRejectRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.RejectInbox);

        var document = await documentStore.GetAsync(organizationId, provider.Code, id, ct)
            ?? throw new InvalidOperationException("The EDO inbox document was not found in the current organization/provider scope.");

        if (document.Direction != EdoDirection.INBOX.ToString()
            || string.IsNullOrWhiteSpace(document.ProviderDocumentId))
            throw new InvalidOperationException("The EDO document is not an inbox document ready for rejection.");

        if (provider.Code == EdoProviderCode.EDOCS
            && !string.Equals(document.ProviderStatusCode, "sended", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Edocs inbox rejection is allowed only when provider status is 'sended'.");

        if (IsTerminal(document.Status))
            throw new InvalidOperationException("A terminal EDO inbox document cannot be rejected again.");

        if (provider.Code == EdoProviderCode.EDOCS
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("Edocs inbox rejection requires PreparedPkcs7.");

        var reason = RequireText(request.Reason, nameof(request.Reason));
        var idempotencyKey = RequireText(request.IdempotencyKey, nameof(request.IdempotencyKey));
        var scopedKey = $"{id}:{idempotencyKey}";
        var existing = await documentStore.FindByIdempotencyAsync(
            organizationId,
            provider.Code,
            RejectOperation,
            scopedKey,
            ct);
        if (existing is not null)
        {
            if (existing.Status == EdoDocumentStatusCode.PENDING.ToString()
                && string.IsNullOrWhiteSpace(request.SigningSessionId))
                throw new InvalidOperationException("An EDO inbox rejection is already in progress for this idempotency key.");
        }

        var idempotency = await idempotencyService.TryCreateIdempotencyRecordAsync(
            organizationId,
            provider.Code,
            RejectOperation,
            scopedKey,
            ComputeRejectRequestHash(id, reason, request),
            request.SigningSessionId,
            ct);
        if (idempotency.IsReplay)
        {
            var replayDocument = existing ?? await documentStore.FindByIdempotencyAsync(
                organizationId,
                provider.Code,
                RejectOperation,
                scopedKey,
                ct);
            if (replayDocument is null)
                throw new InvalidOperationException("The completed EDO reject idempotency result is not available locally.");
            return new EdoInboxRejectDto { Document = MapDocument(replayDocument) };
        }

        if (existing is not null)
        {
            if (existing.Status != EdoDocumentStatusCode.PENDING.ToString())
            {
                await idempotencyService.CompleteIdempotencyAsync(
                    organizationId,
                    provider.Code,
                    RejectOperation,
                    idempotency.Key,
                    existing.ProviderDocumentId ?? existing.Id.ToString(),
                    ct);
                return new EdoInboxRejectDto { Document = MapDocument(existing) };
            }

            document = existing;
        }
        else
        {
            var previousStatus = document.Status;
            document.OperationType = RejectOperation;
            document.IdempotencyKey = scopedKey;
            document.RejectReason = reason;
            document.Status = EdoDocumentStatusCode.PENDING.ToString();
            document.ErrorMessage = null;
            document.UpdatedAt = DateTime.UtcNow;
            try
            {
                await ExecuteInTransactionAsync("MarkInboxRejectPending", async () =>
                {
                    await documentStore.UpdateAsync(document, ct);
                    AuditUpdatedDocument(
                        auditLogService,
                        document,
                        provider.Code,
                        "INBOX_REJECT_PENDING",
                        previousStatus,
                        document.Status);
                    await auditLogService.CreateAsync(
                        DocumentTable,
                        document.Id.ToString(),
                        AuditLogOperationTypeConst.Update);
                    return Result.Success();
                }, ct);
            }
            catch
            {
                await idempotencyService.MarkFailedAsync(
                    organizationId,
                    provider.Code,
                    RejectOperation,
                    idempotency.Key,
                    ct: CancellationToken.None);
                throw;
            }
        }

        var isDidoxChallenge = provider.Code == EdoProviderCode.DIDOX
            && string.IsNullOrWhiteSpace(request.SigningSessionId)
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7)
            && string.IsNullOrWhiteSpace(request.SignatureHex);

        if (isDidoxChallenge)
        {
            EdoInboxRejectDto challenge;
            try
            {
                challenge = await provider.RejectInboxAsync(
                    document.DocumentType,
                    document.ProviderDocumentId!,
                    CopyRejectRequest(request, reason),
                    ct);
            }
            catch (Exception ex)
            {
                await MarkRejectFailureAsync(document, ex, ct);
                await idempotencyService.MarkFailedAsync(
                    organizationId,
                    provider.Code,
                    RejectOperation,
                    idempotency.Key,
                    ct: CancellationToken.None);
                throw;
            }
            var providerSession = challenge.SigningSession
                ?? throw new EdoCapabilityUnavailableException(
                    provider.Code.ToString(),
                    EdoCapabilityKind.RejectInbox.ToString(),
                    EdoCapabilityStatus.UNKNOWN.ToString());

            ApplicationSigningSession? session = null;
            await ExecuteInTransactionAsync("CreateInboxSigningSession", async () =>
            {
                session = await signingSessionStore.CreateAsync(
                    organizationId,
                    provider.Code,
                    document.Id,
                    providerSession.SigningMode,
                    providerSession.ExpiresAt ?? DateTimeOffset.UtcNow.Add(SigningSessionLifetime),
                    ct);
                auditLogService.SetNewValues(new
                {
                    operation = "INBOX_REJECT_SESSION_CREATED",
                    organizationId,
                    providerCode = provider.Code.ToString(),
                    documentId = document.Id,
                    sessionId = session.SessionId,
                    status = "CREATED"
                });
                await auditLogService.CreateAsync(
                    SigningSessionTable,
                    session.SessionId,
                    AuditLogOperationTypeConst.Create);
                await idempotencyService.SetReferenceAsync(
                    organizationId,
                    provider.Code,
                    RejectOperation,
                    idempotency.Key,
                    session.SessionId,
                    ct);
                return Result.Success();
            }, ct);

            return new EdoInboxRejectDto
            {
                Document = MapDocument(document),
                SigningSession = new EdoSigningSessionDto
                {
                    SessionId = session!.SessionId,
                    SigningMode = session.SigningMode,
                    DocumentId = document.Id.ToString(CultureInfo.InvariantCulture),
                    Payload = providerSession.Payload,
                    PayloadFormat = providerSession.PayloadFormat,
                    ExpiresAt = session.ExpiresAt
                }
            };
        }

        if (provider.Code == EdoProviderCode.DIDOX
            && (string.IsNullOrWhiteSpace(request.SigningSessionId)
                || string.IsNullOrWhiteSpace(request.PreparedPkcs7)
                || string.IsNullOrWhiteSpace(request.SignatureHex)))
            throw new InvalidOperationException("Didox inbox rejection requires a signing session, PreparedPkcs7 and SignatureHex.");

        if (!string.IsNullOrWhiteSpace(request.SigningSessionId))
        {
            await ExecuteInTransactionAsync("ConsumeInboxSigningSession", async () =>
            {
                await signingSessionStore.ConsumeAsync(
                    organizationId,
                    provider.Code,
                    document.Id,
                    request.SigningSessionId,
                    ct);
                auditLogService.SetOldValues(new
                {
                    operation = "INBOX_REJECT_SESSION_CONSUMED",
                    organizationId,
                    providerCode = provider.Code.ToString(),
                    documentId = document.Id,
                    sessionId = request.SigningSessionId,
                    status = "CREATED"
                });
                auditLogService.SetNewValues(new
                {
                    operation = "INBOX_REJECT_SESSION_CONSUMED",
                    organizationId,
                    providerCode = provider.Code.ToString(),
                    documentId = document.Id,
                    sessionId = request.SigningSessionId,
                    status = "CONSUMED"
                });
                await auditLogService.CreateAsync(
                    SigningSessionTable,
                    request.SigningSessionId,
                    AuditLogOperationTypeConst.Update);
                return Result.Success();
            }, ct);
        }

        EdoInboxRejectDto providerResult;
        try
        {
            providerResult = await provider.RejectInboxAsync(
                document.DocumentType,
                document.ProviderDocumentId!,
                CopyRejectRequest(request, reason),
                ct);
        }
        catch (Exception ex)
        {
            await MarkRejectFailureAsync(document, ex, ct);
            await idempotencyService.MarkFailedAsync(
                organizationId,
                provider.Code,
                RejectOperation,
                idempotency.Key,
                ct: CancellationToken.None);
            throw;
        }

        var oldStatusBeforeResult = document.Status;
        try
        {
            document = await documentStore.GetAsync(organizationId, provider.Code, id, CancellationToken.None)
                ?? throw new InvalidOperationException("The EDO inbox document disappeared during rejection.");
            document.Status = providerResult.Document.Status.Code.ToString();
            document.ProviderStatusCode = providerResult.Document.Status.ProviderStatusCode;
            document.UpdatedAt = DateTime.UtcNow;
            await ExecuteInTransactionAsync("PersistInboxRejectResult", async () =>
            {
                await documentStore.UpdateAsync(document, CancellationToken.None);
                AuditUpdatedDocument(
                    auditLogService,
                    document,
                    provider.Code,
                    "INBOX_REJECT_RESULT",
                    oldStatusBeforeResult,
                    document.Status);
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                await idempotencyService.CompleteIdempotencyAsync(
                    organizationId,
                    provider.Code,
                    RejectOperation,
                    idempotency.Key,
                    document.ProviderDocumentId ?? document.Id.ToString(),
                    CancellationToken.None);
                return Result.Success();
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            document.Status = EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString();
            document.ErrorMessage = "Remote reject succeeded but local status persistence requires reconciliation.";
            document.UpdatedAt = DateTime.UtcNow;
            try
            {
                await ExecuteInTransactionAsync("PersistInboxRejectReconciliation", async () =>
                {
                    await documentStore.UpdateAsync(document, CancellationToken.None);
                    AuditUpdatedDocument(
                        auditLogService,
                        document,
                        provider.Code,
                        "INBOX_REJECT_RECONCILIATION",
                        oldStatusBeforeResult,
                        document.Status);
                    await auditLogService.CreateAsync(
                        DocumentTable,
                        document.Id.ToString(),
                        AuditLogOperationTypeConst.Update);
                    await idempotencyService.MarkFailedAsync(
                        organizationId,
                        provider.Code,
                        RejectOperation,
                        idempotency.Key,
                        document.ProviderDocumentId,
                        CancellationToken.None);
                    return Result.Success();
                }, CancellationToken.None);
            }
            catch (Exception reconciliationException)
            {
                logger.LogCritical(reconciliationException, "Failed to persist EDO inbox reject reconciliation state for {DocumentId}.", id);
            }
            logger.LogCritical(ex, "EDO inbox reject succeeded remotely but local persistence failed for {DocumentId}.", id);
            throw;
        }

        return new EdoInboxRejectDto { Document = MapDocument(document) };
    }

    public async Task<EdoFileDto> GetFileAsync(long id, CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.GetFile);
        var document = await documentStore.GetAsync(organizationId, provider.Code, id, ct)
            ?? throw new InvalidOperationException("The EDO document was not found in the current organization/provider scope.");
        if (string.IsNullOrWhiteSpace(document.ProviderDocumentId))
            throw new InvalidOperationException("The EDO document has no provider document ID.");

        EdoFileDto file;
        try
        {
            file = await provider.GetFileAsync(document.DocumentType, document.ProviderDocumentId!, ct);
        }
        catch (Exception ex)
        {
            document.ErrorMessage = "EDO file download failed; reconciliation or a later manual download may be required.";
            document.UpdatedAt = DateTime.UtcNow;
            try
            {
                await ExecuteInTransactionAsync("PersistFileDownloadFailure", async () =>
                {
                    await documentStore.UpdateAsync(document, CancellationToken.None);
                    AuditUpdatedDocument(
                        auditLogService,
                        document,
                        provider.Code,
                        "FILE_DOWNLOAD_FAILURE",
                        document.Status,
                        document.Status);
                    await auditLogService.CreateAsync(
                        DocumentTable,
                        document.Id.ToString(),
                        AuditLogOperationTypeConst.Update);
                    return Result.Success();
                }, CancellationToken.None);
            }
            catch (Exception persistenceException)
            {
                logger.LogCritical(persistenceException, "Failed to record EDO file download failure for {DocumentId}.", id);
            }
            logger.LogWarning(ex, "EDO file download failed for {DocumentId}.", id);
            throw;
        }
        return new EdoFileDto
        {
            Id = id,
            DocumentId = id,
            ProviderFileId = file.ProviderFileId,
            FileName = SanitizeFileName(file.FileName, id),
            ContentType = ValidateContentType(file.ContentType),
            Length = file.Length,
            Content = file.Content
        };
    }

    public async Task<EdoDocumentStatusDto> GetStatusAsync(
        long id,
        EdoDirection direction,
        CancellationToken ct = default)
        => (await reconciliationService.ReconcileAsync(id, direction, ct)).Status;

    private async Task<EdoDocument> UpsertInboxDocumentAsync(
        int organizationId,
        EdoProviderCode providerCode,
        EdoDocumentDto providerDocument,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(providerDocument.ProviderDocumentId))
            throw new IntegrationHttpException("The provider inbox response did not include a document ID.", 502);

        var document = await documentStore.FindByProviderDocumentIdAsync(
            organizationId, providerCode, providerDocument.ProviderDocumentId, ct);
        if (document is null)
        {
            document = new EdoDocument
            {
                OrganizationId = organizationId,
                Provider = providerCode.ToString(),
                Direction = EdoDirection.INBOX.ToString(),
                InternalDocumentType = "EDO_INBOX",
                InternalDocumentId = 0,
                ProviderDocumentId = providerDocument.ProviderDocumentId,
                DocumentType = providerDocument.DocumentType,
                DocumentNumber = providerDocument.DocumentNumber,
                DocumentDate = providerDocument.DocumentDate,
                Status = providerDocument.Status.Code.ToString(),
                ProviderStatusCode = providerDocument.Status.ProviderStatusCode,
                OperationType = InboxSyncOperation,
                CreatedAt = DateTime.UtcNow
            };
            await documentStore.AddAsync(document, ct);
            AuditCreatedDocument(auditLogService, document, providerCode, "INBOX_SYNC_CREATE");
            await auditLogService.CreateAsync(
                DocumentTable,
                document.Id.ToString(),
                AuditLogOperationTypeConst.Create);
        }
        else
        {
            document.Direction = EdoDirection.INBOX.ToString();
            document.DocumentType = providerDocument.DocumentType;
            document.DocumentNumber = providerDocument.DocumentNumber;
            document.DocumentDate = providerDocument.DocumentDate;
            document.Status = providerDocument.Status.Code.ToString();
            document.ProviderStatusCode = providerDocument.Status.ProviderStatusCode;
            document.UpdatedAt = DateTime.UtcNow;
            var oldStatus = document.Status;
            await documentStore.UpdateAsync(document, ct);
            AuditUpdatedDocument(
                auditLogService,
                document,
                providerCode,
                "INBOX_SYNC_UPDATE",
                oldStatus,
                document.Status);
            await auditLogService.CreateAsync(
                DocumentTable,
                document.Id.ToString(),
                AuditLogOperationTypeConst.Update);
        }

        return document;
    }

    private async Task MarkRejectFailureAsync(EdoDocument document, Exception exception, CancellationToken ct)
    {
        var remoteStateUnknown = exception is OperationCanceledException
            or HttpRequestException
            or IntegrationHttpException { StatusCode: 408 or 429 or >= 500 };
        var oldStatus = document.Status;
        document.Status = remoteStateUnknown
            ? EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString()
            : EdoDocumentStatusCode.FAILED.ToString();
        document.ErrorMessage = remoteStateUnknown
            ? "EDO reject outcome is unknown after a transport failure; reconciliation is required."
            : exception.Message;
        document.UpdatedAt = DateTime.UtcNow;
        try
        {
            await ExecuteInTransactionAsync("PersistInboxRejectFailure", async () =>
            {
                await documentStore.UpdateAsync(document, ct);
                AuditUpdatedDocument(
                    auditLogService,
                    document,
                    EdoProviderCodeFromDocument(document),
                    "INBOX_REJECT_FAILURE",
                    oldStatus,
                    document.Status);
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                return Result.Success();
            }, ct);
        }
        catch (Exception persistenceException)
        {
            logger.LogCritical(persistenceException, "Failed to persist EDO reject failure for {DocumentId}.", document.Id);
        }
    }

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new EdoOrganizationScopeRequiredException();

    private static void EnsureCapability(IEdoProvider provider, EdoCapabilityKind capability)
    {
        var status = provider.Capabilities.Capabilities.SingleOrDefault(item => item.Kind == capability)?.Status
            ?? EdoCapabilityStatus.UNKNOWN;
        if (status != EdoCapabilityStatus.SUPPORTED)
            throw new EdoCapabilityUnavailableException(provider.Code.ToString(), capability.ToString(), status.ToString());
    }

    private static bool IsTerminal(string status) => status is nameof(EdoDocumentStatusCode.REJECTED)
        or nameof(EdoDocumentStatusCode.SIGNED)
        or nameof(EdoDocumentStatusCode.COMPLETED)
        or nameof(EdoDocumentStatusCode.CANCELLED)
        or nameof(EdoDocumentStatusCode.FAILED)
        or nameof(EdoDocumentStatusCode.RECONCILIATION_REQUIRED);

    private static string RequireText(string? value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"{name} is required.") : value.Trim();

    private static EdoInboxRejectRequestDto CopyRejectRequest(EdoInboxRejectRequestDto request, string reason) => new()
    {
        Reason = reason,
        IdempotencyKey = request.IdempotencyKey,
        SigningSessionId = request.SigningSessionId,
        PreparedPkcs7 = request.PreparedPkcs7,
        SignatureHex = request.SignatureHex
    };

    private static EdoDocumentDto MapDocument(EdoDocument document, EdoDocumentDto? providerDocument = null) => new()
    {
        Id = document.Id,
        ProviderDocumentId = document.ProviderDocumentId,
        Direction = ParseDirection(document.Direction),
        DocumentType = document.DocumentType,
        DocumentNumber = document.DocumentNumber,
        DocumentDate = document.DocumentDate,
        Status = new EdoDocumentStatusDto
        {
            Code = ParseStatus(document.Status),
            LocalCode = ParseStatus(document.Status),
            ProviderStatusCode = document.ProviderStatusCode,
            IsReconciliationRequired = document.Status == nameof(EdoDocumentStatusCode.RECONCILIATION_REQUIRED)
        },
        Seller = providerDocument?.Seller,
        Buyer = providerDocument?.Buyer,
        TotalAmount = providerDocument?.TotalAmount,
        CurrencyCode = providerDocument?.CurrencyCode,
        CreatedAt = document.CreatedAt,
        UpdatedAt = document.UpdatedAt
    };

    private static string SanitizeFileName(string? fileName, long documentId)
    {
        var candidate = string.IsNullOrWhiteSpace(fileName) ? $"edo-document-{documentId}.pdf" : Path.GetFileName(fileName);
        return string.IsNullOrWhiteSpace(candidate) ? $"edo-document-{documentId}.pdf" : candidate;
    }

    private static string ValidateContentType(string? contentType) =>
        string.IsNullOrWhiteSpace(contentType) || contentType.Contains('\n') || contentType.Contains('\r')
            ? "application/octet-stream"
            : contentType;

    private static string ComputeRejectRequestHash(
        long documentId,
        string reason,
        EdoInboxRejectRequestDto request)
    {
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(
                $"{RejectOperation}|{documentId}|{reason}|{request.IdempotencyKey}")));
    }

    private static EdoDirection ParseDirection(string value) =>
        Enum.TryParse<EdoDirection>(value, true, out var result) ? result : EdoDirection.INBOX;

    private static EdoDocumentStatusCode ParseStatus(string value) =>
        Enum.TryParse<EdoDocumentStatusCode>(value, true, out var result) ? result : EdoDocumentStatusCode.UNKNOWN;

    private static void AuditCreatedDocument(
        IAuditLogService auditLogService,
        EdoDocument document,
        EdoProviderCode providerCode,
        string operation) =>
        auditLogService.SetNewValues(new
        {
            operation,
            organizationId = document.OrganizationId,
            providerCode = providerCode.ToString(),
            documentId = document.Id,
            status = document.Status
        });

    private static void AuditUpdatedDocument(
        IAuditLogService auditLogService,
        EdoDocument document,
        EdoProviderCode providerCode,
        string operation,
        string oldStatus,
        string newStatus)
    {
        auditLogService.SetOldValues(new
        {
            operation,
            organizationId = document.OrganizationId,
            providerCode = providerCode.ToString(),
            documentId = document.Id,
            status = oldStatus
        });
        auditLogService.SetNewValues(new
        {
            operation,
            organizationId = document.OrganizationId,
            providerCode = providerCode.ToString(),
            documentId = document.Id,
            status = newStatus
        });
    }

    private static EdoProviderCode EdoProviderCodeFromDocument(EdoDocument document) =>
        Enum.TryParse<EdoProviderCode>(document.Provider, out var providerCode)
            ? providerCode
            : throw new InvalidOperationException("The stored EDO document contains an invalid provider code.");
}
