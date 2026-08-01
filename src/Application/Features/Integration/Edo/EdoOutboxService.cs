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
using ApplicationSigningSession = Application.Abstractions.Integration.Edo.EdoDocumentSigningSession;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Integration.Edo;

public sealed class EdoOutboxService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoDocumentStore documentStore,
    IEdoDocumentSigningSessionStore signingSessionStore,
    IEdoIdempotencyService idempotencyService,
    IAuditLogService auditLogService,
    ILogger<EdoOutboxService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoOutboxService
{
    private const string CreateOperationType = "FACTURA_CREATE";
    private const string SignOperationType = "OUTBOX_SIGN";
    private const string DocumentTable = "edo_document";
    private const string SigningSessionTable = "edo_document_signing_session";
    private static readonly TimeSpan DocumentSigningSessionLifetime = TimeSpan.FromMinutes(5);

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.CreateFactura);

        var idempotencyKey = RequireIdempotencyKey(request.IdempotencyKey);
        var idempotency = await idempotencyService.TryCreateIdempotencyRecordAsync(
            organizationId,
            provider.Code,
            CreateOperationType,
            idempotencyKey,
            ComputeCreateRequestHash(request),
            ct: ct);
        if (idempotency.IsReplay)
        {
            var replayDocument = !string.IsNullOrWhiteSpace(idempotency.ResultDocumentId)
                ? await documentStore.FindByProviderDocumentIdAsync(
                    organizationId,
                    provider.Code,
                    idempotency.ResultDocumentId,
                    ct)
                : null;
            replayDocument ??= await documentStore.FindByIdempotencyAsync(
                organizationId,
                provider.Code,
                CreateOperationType,
                idempotencyKey,
                ct);
            if (replayDocument is null)
                throw new InvalidOperationException("The completed EDO idempotency result is not available locally.");

            return new EdoOutboxCreateDto
            {
                Document = MapDocument(replayDocument),
                IsReplay = true
            };
        }

        var existing = await documentStore.FindByIdempotencyAsync(
            organizationId,
            provider.Code,
            CreateOperationType,
            idempotencyKey,
            ct);
        if (existing is not null)
        {
            if (existing.Status == EdoDocumentStatusCode.PENDING.ToString())
                throw new InvalidOperationException("An EDO factura creation is already in progress for this idempotency key.");

            await idempotencyService.CompleteIdempotencyAsync(
                organizationId,
                provider.Code,
                CreateOperationType,
                idempotency.Key,
                existing.ProviderDocumentId ?? existing.Id.ToString(),
                ct);

            return new EdoOutboxCreateDto
            {
                Document = MapDocument(existing),
                IsReplay = true
            };
        }

        var document = new EdoDocument
        {
            OrganizationId = organizationId,
            Provider = provider.Code.ToString(),
            Direction = EdoDirection.OUTBOX.ToString(),
            InternalDocumentType = RequireText(request.InternalDocumentType, nameof(request.InternalDocumentType)),
            InternalDocumentId = request.InternalDocumentId,
            DocumentType = "FACTURA",
            DocumentNumber = request.DocumentNumber,
            DocumentDate = request.DocumentDate,
            Status = EdoDocumentStatusCode.PENDING.ToString(),
            OperationType = CreateOperationType,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await ExecuteInTransactionAsync("CreatePendingDocument", async () =>
            {
                await documentStore.AddAsync(document, ct);
                AuditCreatedDocument(auditLogService, document, provider.Code, "FACTURA_CREATE_PENDING");
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Create);
                return Result.Success();
            }, ct);
        }
        catch (UniqueConstraintViolationException)
        {
            var concurrent = await documentStore.FindByIdempotencyAsync(
                organizationId,
                provider.Code,
                CreateOperationType,
                idempotencyKey,
                ct);
            if (concurrent is not null)
            {
                if (concurrent.Status == EdoDocumentStatusCode.PENDING.ToString())
                    throw new InvalidOperationException("An EDO factura creation is already in progress for this idempotency key.");

                return new EdoOutboxCreateDto
                {
                    Document = MapDocument(concurrent),
                    IsReplay = true
                };
            }

            throw;
        }

        var providerScopedKey = CreateProviderScopedKey(provider.Code, idempotencyKey);
        var providerRequest = CopyWithIdempotencyKey(request, providerScopedKey);
        EdoOutboxCreateDto providerResult;
        try
        {
            providerResult = await provider.CreateFacturaAsync(providerRequest, ct);
        }
        catch (Exception ex)
        {
            await MarkCreateFailureAsync(document.Id, organizationId, provider.Code, providerScopedKey, idempotency.Key, null, ex);
            throw;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(providerResult.Document.ProviderDocumentId))
            {
                throw new InvalidOperationException("The provider did not return a provider factura document ID.");
            }

            document = await documentStore.GetAsync(organizationId, provider.Code, document.Id, CancellationToken.None)
                ?? throw new InvalidOperationException("The pending EDO document disappeared before result persistence.");

            var oldStatus = document.Status;
            ApplyProviderResult(document, providerResult.Document);
            await ExecuteInTransactionAsync("PersistCreateResult", async () =>
            {
                await documentStore.UpdateAsync(document, CancellationToken.None);
                AuditUpdatedDocument(
                    auditLogService,
                    document,
                    provider.Code,
                    "FACTURA_CREATE_RESULT",
                    oldStatus,
                    document.Status);
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                await idempotencyService.CompleteIdempotencyAsync(
                    organizationId,
                    provider.Code,
                    CreateOperationType,
                    idempotency.Key,
                    document.ProviderDocumentId ?? document.Id.ToString(),
                    CancellationToken.None);
                return Result.Success();
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await MarkCreateFailureAsync(
                document.Id,
                organizationId,
                provider.Code,
                providerScopedKey,
                idempotency.Key,
                providerResult.Document.ProviderDocumentId,
                ex);
            throw;
        }

        return new EdoOutboxCreateDto
        {
            Document = MapDocument(document),
            IsReplay = providerResult.IsReplay
        };
    }

    public async Task<EdoOutboxSignDto> SignAsync(
        long id,
        EdoOutboxSignRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.SignOutbox);

        var document = await documentStore.GetAsync(organizationId, provider.Code, id, ct)
            ?? throw new InvalidOperationException("The EDO document was not found in the current organization/provider scope.");

        if (document.Direction != EdoDirection.OUTBOX.ToString()
            || string.IsNullOrWhiteSpace(document.ProviderDocumentId))
        {
            throw new InvalidOperationException("The EDO document is not ready for signing.");
        }

        if (provider.Code != EdoProviderCode.FAKTURA && document.LegacyDocumentId is null)
        {
            throw new IntegrationHttpException(
                "The provider document has no numeric legacy ID required by the signing operation.",
                502);
        }

        if (document.Status is nameof(EdoDocumentStatusCode.FAILED)
            or nameof(EdoDocumentStatusCode.RECONCILIATION_REQUIRED))
        {
            throw new InvalidOperationException("The EDO document requires reconciliation before signing.");
        }

        var signIdempotencyKey = RequireIdempotencyKey(
            request.IdempotencyKey,
            "IdempotencyKey is required for EDO outbox signing.");
        var signIdempotency = await idempotencyService.TryCreateIdempotencyRecordAsync(
            organizationId,
            provider.Code,
            SignOperationType,
            signIdempotencyKey,
            ComputeSignRequestHash(id, request),
            request.SigningSessionId,
            ct);
        if (signIdempotency.IsReplay)
            return new EdoOutboxSignDto { Document = MapDocument(document) };

        var isDidoxChallengeRequest = provider.Code == EdoProviderCode.DIDOX
            && string.IsNullOrWhiteSpace(request.SigningSessionId)
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7)
            && string.IsNullOrWhiteSpace(request.SignatureHex);

        if (isDidoxChallengeRequest)
        {
            EdoOutboxSignDto challenge;
            try
            {
                challenge = await provider.SignOutboxAsync(document.LegacyDocumentId!.Value, request, ct);
            }
            catch
            {
                await idempotencyService.MarkFailedAsync(
                    organizationId,
                    provider.Code,
                    SignOperationType,
                    signIdempotency.Key,
                    ct: CancellationToken.None);
                throw;
            }
            var providerSession = challenge.SigningSession
                ?? throw new EdoCapabilityUnavailableException(
                    provider.Code.ToString(),
                    "SignChallenge",
                    EdoCapabilityStatus.UNKNOWN.ToString());

            ApplicationSigningSession? session = null;
            await ExecuteInTransactionAsync("CreateDocumentSigningSession", async () =>
            {
                session = await signingSessionStore.CreateAsync(
                    organizationId,
                    provider.Code,
                    document.Id,
                    providerSession.SigningMode,
                    providerSession.ExpiresAt ?? DateTimeOffset.UtcNow.Add(DocumentSigningSessionLifetime),
                    ct);
                auditLogService.SetNewValues(new
                {
                    operation = "OUTBOX_SIGN_SESSION_CREATED",
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
                    SignOperationType,
                    signIdempotency.Key,
                    session.SessionId,
                    ct);
                return Result.Success();
            }, ct);

            return new EdoOutboxSignDto
            {
                Document = MapDocument(document),
                SigningSession = new EdoSigningSessionDto
                {
                    SessionId = session!.SessionId,
                    SigningMode = session.SigningMode,
                    DocumentId = document.Id.ToString(),
                    Payload = providerSession.Payload,
                    PayloadFormat = providerSession.PayloadFormat,
                    ExpiresAt = session.ExpiresAt
                }
            };
        }

        if (provider.Code == EdoProviderCode.DIDOX)
        {
            if (string.IsNullOrWhiteSpace(request.SigningSessionId))
                throw new EdoAuthSigningSessionException();
            if (string.IsNullOrWhiteSpace(request.PreparedPkcs7)
                || string.IsNullOrWhiteSpace(request.SignatureHex))
            {
                throw new InvalidOperationException("Didox signing requires PreparedPkcs7 and SignatureHex.");
            }
        }

        if (provider.Code == EdoProviderCode.EDOCS
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7))
        {
            throw new InvalidOperationException("Edocs signing requires PreparedPkcs7.");
        }

        if (!string.IsNullOrWhiteSpace(request.SigningSessionId))
        {
            await ExecuteInTransactionAsync("ConsumeDocumentSigningSession", async () =>
            {
                await signingSessionStore.ConsumeAsync(
                    organizationId,
                    provider.Code,
                    document.Id,
                    request.SigningSessionId,
                    ct);
                auditLogService.SetOldValues(new
                {
                    operation = "OUTBOX_SIGN_SESSION_CONSUMED",
                    organizationId,
                    providerCode = provider.Code.ToString(),
                    documentId = document.Id,
                    sessionId = request.SigningSessionId,
                    status = "CREATED"
                });
                auditLogService.SetNewValues(new
                {
                    operation = "OUTBOX_SIGN_SESSION_CONSUMED",
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

        EdoOutboxSignDto providerResult;
        try
        {
            providerResult = provider.Code == EdoProviderCode.FAKTURA
                ? await provider.SignOutboxAsync(document.ProviderDocumentId!, request, ct)
                : await provider.SignOutboxAsync(document.LegacyDocumentId!.Value, request, ct);
        }
        catch
        {
            await idempotencyService.MarkFailedAsync(
                organizationId,
                provider.Code,
                SignOperationType,
                signIdempotency.Key,
                ct: CancellationToken.None);
            throw;
        }

        try
        {
            document = await documentStore.GetAsync(organizationId, provider.Code, id, CancellationToken.None)
                ?? throw new InvalidOperationException("The EDO document disappeared during signing.");
            var oldStatus = document.Status;
            ApplyProviderResult(document, providerResult.Document);
            await ExecuteInTransactionAsync("PersistSignResult", async () =>
            {
                await documentStore.UpdateAsync(document, CancellationToken.None);
                AuditUpdatedDocument(
                    auditLogService,
                    document,
                    provider.Code,
                    "OUTBOX_SIGN_RESULT",
                    oldStatus,
                    document.Status);
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                await idempotencyService.CompleteIdempotencyAsync(
                    organizationId,
                    provider.Code,
                    SignOperationType,
                    signIdempotency.Key,
                    document.ProviderDocumentId ?? document.Id.ToString(),
                    CancellationToken.None);
                return Result.Success();
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "EDO sign succeeded remotely but common EdoDocument {DocumentId} local status update failed. Manual reconciliation required.",
                id);
            await idempotencyService.MarkFailedAsync(
                organizationId,
                provider.Code,
                SignOperationType,
                signIdempotency.Key,
                document.ProviderDocumentId,
                CancellationToken.None);
            throw;
        }

        return new EdoOutboxSignDto { Document = MapDocument(document) };
    }

    private async Task MarkCreateFailureAsync(
        long documentId,
        int organizationId,
        EdoProviderCode providerCode,
        string providerScopedKey,
        string idempotencyKey,
        string? fallbackProviderDocumentId,
        Exception originalException)
    {
        try
        {
            await ExecuteInTransactionAsync("PersistCreateFailure", async () =>
            {
                var providerDocumentId = await documentStore.FindProviderResultIdAsync(
                    organizationId,
                    providerScopedKey,
                    CancellationToken.None) ?? fallbackProviderDocumentId;
                var document = await documentStore.GetAsync(
                    organizationId,
                    providerCode,
                    documentId,
                    CancellationToken.None);
                if (document is null)
                {
                    await idempotencyService.MarkFailedAsync(
                        organizationId,
                        providerCode,
                        CreateOperationType,
                        idempotencyKey,
                        providerDocumentId,
                        CancellationToken.None);
                    return Result.Success();
                }

                var oldStatus = document.Status;
                document.ProviderDocumentId = providerDocumentId;
                document.Status = string.IsNullOrWhiteSpace(providerDocumentId)
                    ? EdoDocumentStatusCode.FAILED.ToString()
                    : EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString();
                document.ErrorMessage = string.IsNullOrWhiteSpace(providerDocumentId)
                    ? "EDO factura creation failed."
                    : "Remote factura creation succeeded but local reconciliation is required.";
                document.UpdatedAt = DateTime.UtcNow;
                await documentStore.UpdateAsync(document, CancellationToken.None);
                AuditUpdatedDocument(
                    auditLogService,
                    document,
                    providerCode,
                    "FACTURA_CREATE_FAILURE",
                    oldStatus,
                    document.Status);
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                await idempotencyService.MarkFailedAsync(
                    organizationId,
                    providerCode,
                    CreateOperationType,
                    idempotencyKey,
                    providerDocumentId,
                    CancellationToken.None);

                if (!string.IsNullOrWhiteSpace(providerDocumentId))
                {
                    logger.LogCritical(
                        originalException,
                        "EDO factura creation succeeded remotely with ProviderDocumentId {ProviderDocumentId}, but common local persistence requires reconciliation for EdoDocument {DocumentId}.",
                        providerDocumentId,
                        documentId);
                }

                return Result.Success();
            }, CancellationToken.None);
        }
        catch (Exception reconciliationException)
        {
            logger.LogCritical(
                reconciliationException,
                "Failed to persist EDO factura reconciliation state for EdoDocument {DocumentId}.",
                documentId);
        }
    }

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

    private static void EnsureCapability(IEdoProvider provider, EdoCapabilityKind capability)
    {
        var status = provider.Capabilities.Capabilities
            .Single(item => item.Kind == capability)
            .Status;
        if (status != EdoCapabilityStatus.SUPPORTED)
        {
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(), capability.ToString(), status.ToString());
        }
    }

    private static EdoOutboxFacturaCreateRequestDto CopyWithIdempotencyKey(
        EdoOutboxFacturaCreateRequestDto request,
        string idempotencyKey) => new()
        {
            InternalDocumentId = request.InternalDocumentId,
            InternalDocumentType = request.InternalDocumentType,
            Seller = request.Seller,
            Buyer = request.Buyer,
            DocumentNumber = request.DocumentNumber,
            DocumentDate = request.DocumentDate,
            ContractNumber = request.ContractNumber,
            ContractDate = request.ContractDate,
            Empowerment = request.Empowerment,
            Lines = request.Lines,
            IdempotencyKey = idempotencyKey
        };

    private static string CreateProviderScopedKey(EdoProviderCode providerCode, string key)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
        return $"EDO_{providerCode}*{CreateOperationType}*{hash}";
    }

    private static void ApplyProviderResult(EdoDocument document, EdoDocumentDto providerDocument)
    {
        document.LegacyDocumentId = providerDocument.LegacyDocumentId ?? document.LegacyDocumentId;
        document.ProviderDocumentId = providerDocument.ProviderDocumentId ?? document.ProviderDocumentId;
        document.Status = providerDocument.Status.Code.ToString();
        document.ErrorMessage = null;
        document.UpdatedAt = DateTime.UtcNow;
    }

    private static EdoDocumentDto MapDocument(EdoDocument document) => new()
    {
        Id = document.Id,
        ProviderDocumentId = document.ProviderDocumentId,
        Direction = Enum.Parse<EdoDirection>(document.Direction),
        DocumentType = document.DocumentType,
        DocumentNumber = document.DocumentNumber,
        DocumentDate = document.DocumentDate,
        Status = MapStatus(document.Status),
        CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(document.CreatedAt, DateTimeKind.Utc)),
        UpdatedAt = document.UpdatedAt is null
            ? null
            : new DateTimeOffset(DateTime.SpecifyKind(document.UpdatedAt.Value, DateTimeKind.Utc))
    };

    private static EdoDocumentStatusDto MapStatus(string status) =>
        Enum.TryParse<EdoDocumentStatusCode>(status, out var code)
            ? new EdoDocumentStatusDto
            {
                Code = code,
                IsTerminal = code is EdoDocumentStatusCode.SIGNED
                    or EdoDocumentStatusCode.COMPLETED
                    or EdoDocumentStatusCode.CANCELLED
                    or EdoDocumentStatusCode.FAILED,
                IsSuccessful = code is EdoDocumentStatusCode.SIGNED
                    or EdoDocumentStatusCode.COMPLETED
            }
            : new EdoDocumentStatusDto { Code = EdoDocumentStatusCode.UNKNOWN };

    private static string RequireIdempotencyKey(
        string key,
        string message = "IdempotencyKey is required for EDO factura creation.") =>
        string.IsNullOrWhiteSpace(key)
            ? throw new InvalidOperationException(message)
            : key.Length > 200
                ? throw new InvalidOperationException("IdempotencyKey must not exceed 200 characters.")
                : key;

    private static string ComputeCreateRequestHash(EdoOutboxFacturaCreateRequestDto request)
    {
        var lines = string.Join(";", request.Lines.Select(line =>
            $"{line.Number}|{line.Name}|{line.Quantity}|{line.Amount}|{line.TaxRate}|{line.TaxAmount}|{line.IsTaxFree}|{string.Join(',', line.MarkingCodeIds.OrderBy(id => id))}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{CreateOperationType}|{request.InternalDocumentType}|{request.InternalDocumentId}|{request.DocumentNumber}|{request.DocumentDate:O}|{request.Seller.TaxIdentifier}|{request.Buyer.TaxIdentifier}|{lines}")));
    }

    private static string ComputeSignRequestHash(long documentId, EdoOutboxSignRequestDto request)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            $"{SignOperationType}|{documentId}|{request.IdempotencyKey}|{request.CertificateSerialNumber}|{request.PreparedPkcs7}|{request.SignatureHex}")));

    private static string RequireText(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for EDO factura creation.")
            : value;

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new EdoOrganizationScopeRequiredException();
}
