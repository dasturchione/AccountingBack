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
using System.Text.Json;
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
        EnsureDateRange(request.DateFrom, request.DateTo);
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.ListInbox);

        if (request.Page < 1 || request.PageSize is < 1 or > 100)
            throw new InvalidOperationException("Inbox page must be at least 1 and page size must be between 1 and 100.");

        var providerResult = await provider.ListInboxAsync(request, ct);
        // Listing is a read-only operation. Local reconciliation/persistence is
        // performed by explicit workflows, never as a side effect of GET.
        var items = await MapReadOnlyProviderItemsAsync(
            organizationId,
            provider,
            providerResult,
            ct);

        return new EdoInboxListDto
        {
            Items = items,
            Page = providerResult.Page,
            PageSize = providerResult.PageSize,
            TotalCount = providerResult.TotalCount,
            TotalDocs = providerResult.TotalDocs,
            TotalPages = providerResult.TotalPages,
            HasNextPage = providerResult.HasNextPage,
            HasPrevPage = providerResult.HasPrevPage,
            HasPreviousPage = providerResult.HasPreviousPage ?? providerResult.HasPrevPage,
            Limit = providerResult.Limit,
            NextPage = providerResult.NextPage,
            PrevPage = providerResult.PrevPage,
            PreviousPage = providerResult.PreviousPage ?? providerResult.PrevPage,
            PagingCounter = providerResult.PagingCounter
        };
    }

    public async Task<EdoInboxListDto> ListDocumentsAsync(
        EdoDocumentQueryDto request,
        CancellationToken ct = default)
    {
        if (request.Scope != EdoDocumentQueryScope.ALL)
        {
            EnsureCategoryMatchesDirection(
                request.Scope == EdoDocumentQueryScope.INBOX
                    ? EdoDirection.INBOX
                    : EdoDirection.OUTBOX,
                request.Category);
        }
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var capability = request.Scope switch
        {
            EdoDocumentQueryScope.INBOX => EdoCapabilityKind.ListInbox,
            EdoDocumentQueryScope.OUTBOX => EdoCapabilityKind.ListOutbox,
            EdoDocumentQueryScope.ALL => EdoCapabilityKind.ListAll,
            _ => EdoCapabilityKind.ListInbox
        };
        EnsureCapability(provider, capability);

        if (request.Page < 1 || request.Limit is < 1 or > 100)
            throw new InvalidOperationException("EDO page must be at least 1 and limit must be between 1 and 100.");
        EnsureDateRange(request.DateFrom, request.DateTo);

        if (request.Scope == EdoDocumentQueryScope.INBOX)
            return await ListInboxAsync(request.ToInboxQuery(), ct);

        var providerResult = await provider.ListDocumentsAsync(request, ct);
        var items = new List<EdoDocumentDto>(providerResult.Items.Count);
        foreach (var providerDocument in providerResult.Items)
        {
            if (string.IsNullOrWhiteSpace(providerDocument.ProviderDocumentId))
            {
                items.Add(providerDocument);
                continue;
            }

            var localDocument = await documentStore.FindByProviderDocumentIdAsync(
                organizationId,
                provider.Code,
                providerDocument.ProviderDocumentId,
                ct);
            items.Add(localDocument is null
                ? MapUnpersistedProviderDocument(providerDocument)
                : MapDocument(localDocument, providerDocument, provider.Code));
        }

        return new EdoInboxListDto
        {
            Items = items,
            Page = providerResult.Page,
            PageSize = providerResult.PageSize,
            TotalCount = providerResult.TotalCount,
            TotalDocs = providerResult.TotalDocs,
            TotalPages = providerResult.TotalPages,
            HasNextPage = providerResult.HasNextPage,
            HasPrevPage = providerResult.HasPrevPage,
            HasPreviousPage = providerResult.HasPreviousPage ?? providerResult.HasPrevPage,
            Limit = providerResult.Limit,
            NextPage = providerResult.NextPage,
            PrevPage = providerResult.PrevPage,
            PreviousPage = providerResult.PreviousPage ?? providerResult.PrevPage,
            PagingCounter = providerResult.PagingCounter
        };
    }

    public async Task<EdoInboxListDto> ListAllDocumentsAsync(
        EdoAllDocumentsQueryDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var includeInbox = request.Category != EdoDocumentCategory.OUTBOX;
        var includeOutbox = request.Category != EdoDocumentCategory.INBOX;
        if (includeInbox)
            EnsureCapability(provider, EdoCapabilityKind.ListInbox);
        if (includeOutbox)
            EnsureCapability(provider, EdoCapabilityKind.ListOutbox);

        if (request.Page < 1 || request.EffectivePageSize is < 1 or > 100)
            throw new InvalidOperationException("EDO page must be at least 1 and page size must be between 1 and 100.");
        EnsureDateRange(request.DateFrom, request.DateTo);

        var pageSize = request.EffectivePageSize;

        // Fetch both directions through their existing provider contracts. The
        // provider ListAll contract is intentionally never used here.
        var firstInboxPage = includeInbox
            ? await provider.ListInboxAsync(request.ToInboxQuery(1), ct)
            : null;
        var firstOutboxPage = includeOutbox
            ? await provider.ListDocumentsAsync(request.ToOutboxQuery(1), ct)
            : null;
        var inboxTotal = firstInboxPage is null
            ? 0
            : RequireProviderTotal(firstInboxPage, provider.Code, EdoDirection.INBOX);
        var outboxTotal = firstOutboxPage is null
            ? 0
            : RequireProviderTotal(firstOutboxPage, provider.Code, EdoDirection.OUTBOX);
        var totalCount = checked(inboxTotal + outboxTotal);
        var totalPages = totalCount == 0
            ? 0
            : checked((totalCount + pageSize - 1) / pageSize);
        var offset = checked((request.Page - 1) * pageSize);
        var items = new List<EdoDocumentDto>(pageSize);

        if (totalCount == 0)
        {
            return new EdoInboxListDto
            {
                Items = items,
                Page = request.Page,
                PageSize = pageSize,
                TotalCount = 0,
                TotalDocs = 0,
                TotalPages = 0,
                HasNextPage = false,
                HasPrevPage = false,
                HasPreviousPage = false,
                NextPage = null,
                PrevPage = null,
                PreviousPage = null,
                PagingCounter = 0
            };
        }

        if (includeInbox && offset < inboxTotal)
        {
            var inboxPageNumber = offset / pageSize + 1;
            var inboxPage = inboxPageNumber == 1
                ? firstInboxPage!
                : await provider.ListInboxAsync(request.ToInboxQuery(inboxPageNumber), ct);
            var inboxItems = await MapReadOnlyProviderItemsAsync(
                organizationId,
                provider,
                inboxPage,
                ct);
            items.AddRange(inboxItems.Skip(offset % pageSize).Take(pageSize));

            if (includeOutbox && outboxTotal > 0 && items.Count < pageSize && offset + pageSize > inboxTotal)
            {
                var outboxItems = await MapReadOnlyProviderItemsAsync(
                    organizationId,
                    provider,
                    firstOutboxPage!,
                    ct);
                items.AddRange(outboxItems.Take(pageSize - items.Count));
            }
        }
        else if (includeOutbox && outboxTotal > 0)
        {
            var outboxOffset = offset - inboxTotal;
            var outboxPageNumber = outboxOffset / pageSize + 1;
            var outboxPage = outboxPageNumber == 1
                ? firstOutboxPage!
                : await provider.ListDocumentsAsync(request.ToOutboxQuery(outboxPageNumber), ct);
            var outboxItems = await MapReadOnlyProviderItemsAsync(
                organizationId,
                provider,
                outboxPage,
                ct);
            items.AddRange(outboxItems.Skip(outboxOffset % pageSize).Take(pageSize));
        }

        return new EdoInboxListDto
        {
            Items = items,
            Page = request.Page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalDocs = totalCount,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPrevPage = request.Page > 1,
            HasPreviousPage = request.Page > 1,
            NextPage = request.Page < totalPages ? request.Page + 1 : null,
            PrevPage = request.Page > 1 ? request.Page - 1 : null,
            PreviousPage = request.Page > 1 ? request.Page - 1 : null,
            PagingCounter = totalCount == 0 ? 0 : offset + 1
        };
    }

    public async Task<EdoDocumentDto> GetDetailsAsync(
        long id,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.GetDetail);

        var document = await documentStore.GetAsync(organizationId, provider.Code, id, ct)
            ?? throw new InvalidOperationException("The EDO document was not found in the current organization/provider scope.");

        if (string.IsNullOrWhiteSpace(document.ProviderDocumentId))
            throw new InvalidOperationException("The EDO document has no provider document ID.");

        var providerDocument = await provider.GetDocumentDetailsAsync(
            ParseDirection(document.Direction),
            document.DocumentType,
            document.ProviderDocumentId,
            ct);

        return MapDocument(document, providerDocument, provider.Code);
    }

    public async Task<EdoInboxSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.Summary);
        return await provider.GetInboxSummaryAsync(ct);
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
    {
        if (id <= 0)
            throw new EdoDocumentNotFoundException();

        return (await reconciliationService.ReconcileAsync(id, direction, ct)).Status;
    }

    public async Task<EdoProviderDocumentStatusResponseDto> GetRemoteOutboxStatusAsync(
        string providerDocumentId,
        CancellationToken ct = default)
    {
        RequireOrganization();
        if (string.IsNullOrWhiteSpace(providerDocumentId))
            throw new EdoProviderDocumentIdentityRequiredException();

        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.GetOutboxStatus);

        var identity = providerDocumentId.Trim();
        var status = await provider.GetOutboxStatusAsync(identity, ct);
        return new EdoProviderDocumentStatusResponseDto
        {
            DocumentIdentity = identity,
            ProviderDocumentId = identity,
            ProviderCode = provider.Code,
            Direction = EdoDirection.OUTBOX,
            Status = status
        };
    }

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
                DocumentDateTime = providerDocument.DocumentDateTime,
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
            document.DocumentDateTime = providerDocument.DocumentDateTime;
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

    private async Task<IReadOnlyCollection<EdoDocumentDto>> MapReadOnlyProviderItemsAsync(
        int organizationId,
        IEdoProvider provider,
        EdoInboxListDto providerResult,
        CancellationToken ct)
    {
        var items = new List<EdoDocumentDto>(providerResult.Items.Count);
        foreach (var providerDocument in providerResult.Items)
        {
            if (string.IsNullOrWhiteSpace(providerDocument.ProviderDocumentId))
            {
                items.Add(providerDocument);
                continue;
            }

            var localDocument = await documentStore.FindByProviderDocumentIdAsync(
                organizationId,
                provider.Code,
                providerDocument.ProviderDocumentId,
                ct);
            items.Add(localDocument is null
                ? MapUnpersistedProviderDocument(providerDocument)
                : MapDocument(localDocument, providerDocument, provider.Code));
        }

        return items;
    }

    private static int RequireProviderTotal(
        EdoInboxListDto response,
        EdoProviderCode providerCode,
        EdoDirection direction) =>
        response.TotalCount
            ?? response.TotalDocs
            ?? throw new IntegrationHttpException(
                $"{providerCode} {direction} response did not contain a total count metadata field.",
                502);

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

    private static void EnsureCategoryMatchesDirection(
        EdoDirection direction,
        EdoDocumentCategory? category)
    {
        var isMismatch = direction == EdoDirection.INBOX
            ? category == EdoDocumentCategory.OUTBOX
            : category == EdoDocumentCategory.INBOX;
        if (!isMismatch)
            return;

        throw new IntegrationHttpException(
            $"Category '{category}' does not match the {direction} EDO endpoint.",
            400);
    }

    private static void EnsureDateRange(DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            throw new IntegrationHttpException(
                "DateFrom must be earlier than or equal to DateTo.",
                400);
    }

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
        or nameof(EdoDocumentStatusCode.DELETED)
        or nameof(EdoDocumentStatusCode.ARCHIVED)
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

    private static EdoDocumentDto MapDocument(
        EdoDocument document,
        EdoDocumentDto? providerDocument = null,
        EdoProviderCode? providerCode = null)
    {
        var direction = ParseDirection(document.Direction);
        var localStatus = ParseStatus(document.Status);
        var status = providerDocument?.Status ?? new EdoDocumentStatusDto
        {
            Code = localStatus,
            LocalCode = localStatus,
            ProviderStatusCode = document.ProviderStatusCode,
            ProviderRawStatus = document.ProviderStatusCode,
            IsReconciliationRequired = localStatus == EdoDocumentStatusCode.RECONCILIATION_REQUIRED
        };
        var providerFields = providerDocument?.ProviderFields
            ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        var edocsFields = IsEdocs(providerDocument, providerCode)
            ? ReadEdocsCommonFields(providerFields)
            : EdocsCommonFields.Empty;

        return new EdoDocumentDto
        {
            Id = document.Id,
            StatusCheckable = document.Id > 0,
            ProviderCode = providerDocument?.ProviderCode
                ?? providerCode
                ?? ParseProviderCode(document.Provider),
            ProviderDocumentId = document.ProviderDocumentId,
            Direction = providerDocument?.Direction ?? direction,
            Category = providerDocument?.Category ?? MapCategory(direction, status.Code),
            DocumentType = document.DocumentType,
            DocumentNumber = providerDocument?.DocumentNumber
                ?? edocsFields.DocumentNumber
                ?? document.DocumentNumber,
            DocumentDate = providerDocument?.DocumentDate
                ?? edocsFields.DocumentDate
                ?? document.DocumentDate,
            DocumentDateTime = providerDocument?.DocumentDateTime
                ?? document.DocumentDateTime,
            Status = new EdoDocumentStatusDto
            {
                Code = status.Code,
                LocalCode = localStatus,
                ProviderStatusCode = status.ProviderStatusCode ?? document.ProviderStatusCode,
                ProviderRawStatus = status.ProviderRawStatus ?? status.ProviderStatusCode ?? document.ProviderStatusCode,
                Description = status.Description,
                IsTerminal = status.IsTerminal,
                IsSuccessful = status.IsSuccessful,
                CheckedAt = status.CheckedAt,
                IsReconciliationRequired = localStatus == EdoDocumentStatusCode.RECONCILIATION_REQUIRED
                    || status.IsReconciliationRequired
            },
            Empowerment = providerDocument?.Empowerment,
            Seller = providerDocument?.Seller ?? edocsFields.Seller,
            Buyer = providerDocument?.Buyer ?? edocsFields.Buyer,
            TotalAmount = providerDocument?.TotalAmount ?? edocsFields.TotalAmount,
            CurrencyCode = providerDocument?.CurrencyCode,
            MarkingCodes = providerDocument?.MarkingCodes ?? [],
            ProviderFields = providerFields,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static EdoDocumentDto MapUnpersistedProviderDocument(EdoDocumentDto providerDocument)
    {
        var edocsFields = IsEdocs(providerDocument, null)
            ? ReadEdocsCommonFields(providerDocument.ProviderFields)
            : EdocsCommonFields.Empty;

        return new EdoDocumentDto
        {
        // A provider document ID is not a common numeric document ID. Keep it
        // in its dedicated field and make status/detail actions unavailable
        // until a local EdoDocument has been resolved.
        Id = null,
        StatusCheckable = false,
        ProviderCode = providerDocument.ProviderCode,
        ProviderDocumentId = providerDocument.ProviderDocumentId,
        Direction = providerDocument.Direction,
        Category = providerDocument.Category,
        DocumentType = providerDocument.DocumentType,
        DocumentNumber = providerDocument.DocumentNumber ?? edocsFields.DocumentNumber,
        DocumentDate = providerDocument.DocumentDate ?? edocsFields.DocumentDate,
        DocumentDateTime = providerDocument.DocumentDateTime,
        Status = providerDocument.Status,
        Empowerment = providerDocument.Empowerment,
        Seller = providerDocument.Seller ?? edocsFields.Seller,
        Buyer = providerDocument.Buyer ?? edocsFields.Buyer,
        TotalAmount = providerDocument.TotalAmount ?? edocsFields.TotalAmount,
        CurrencyCode = providerDocument.CurrencyCode,
        CreatedAt = providerDocument.CreatedAt,
        UpdatedAt = providerDocument.UpdatedAt,
        MarkingCodes = providerDocument.MarkingCodes,
        ProviderFields = providerDocument.ProviderFields,
        LegacyDocumentId = providerDocument.LegacyDocumentId
        };
    }

    private static bool IsEdocs(EdoDocumentDto? providerDocument, EdoProviderCode? providerCode) =>
        (providerDocument?.ProviderCode ?? providerCode) == EdoProviderCode.EDOCS;

    private static EdocsCommonFields ReadEdocsCommonFields(
        IReadOnlyDictionary<string, JsonElement> fields)
    {
        var seller = ReadParty(
            ReadStringField(fields, "ownerName"),
            ReadStringField(fields, "ownerTin"));

        return new EdocsCommonFields(
            ReadStringField(fields, "docNumber"),
            ReadDateField(fields, "docDate"),
            ReadDecimalField(fields, "totalDocSum"),
            seller,
            ReadBuyer(fields));
    }

    private static EdoPartyDto? ReadBuyer(IReadOnlyDictionary<string, JsonElement> fields)
    {
        if (!fields.TryGetValue("targetTins", out var targetTins)
            || targetTins.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var target in targetTins.EnumerateArray())
        {
            if (target.ValueKind != JsonValueKind.Object
                || !string.Equals(
                    ReadStringProperty(target, "side"),
                    "buyer",
                    StringComparison.OrdinalIgnoreCase))
                continue;

            var party = ReadParty(
                ReadStringProperty(target, "name"),
                ReadStringProperty(target, "tin"));
            if (party is not null)
                return party;
        }

        return null;
    }

    private static EdoPartyDto? ReadParty(string? name, string? taxIdentifier) =>
        string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(taxIdentifier)
            ? null
            : new EdoPartyDto
            {
                Name = name ?? string.Empty,
                TaxIdentifier = taxIdentifier ?? string.Empty
            };

    private static string? ReadStringField(
        IReadOnlyDictionary<string, JsonElement> fields,
        string name) => fields.TryGetValue(name, out var value)
            ? ReadScalarString(value)
            : null;

    private static string? ReadStringProperty(JsonElement objectElement, string name) =>
        objectElement.ValueKind == JsonValueKind.Object
        && objectElement.TryGetProperty(name, out var value)
            ? ReadScalarString(value)
            : null;

    private static string? ReadScalarString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()) ? null : value.GetString()!.Trim(),
        JsonValueKind.Number => value.GetRawText(),
        _ => null
    };

    private static DateOnly? ReadDateField(
        IReadOnlyDictionary<string, JsonElement> fields,
        string name)
    {
        var value = ReadStringField(fields, name);
        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var timestamp)
            ? DateOnly.FromDateTime(timestamp.UtcDateTime)
            : null;
    }

    private static decimal? ReadDecimalField(
        IReadOnlyDictionary<string, JsonElement> fields,
        string name)
    {
        if (!fields.TryGetValue(name, out var value))
            return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
            return number;

        return value.ValueKind == JsonValueKind.String
            && decimal.TryParse(
                value.GetString(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsed)
            ? parsed
            : null;
    }

    private sealed record EdocsCommonFields(
        string? DocumentNumber,
        DateOnly? DocumentDate,
        decimal? TotalAmount,
        EdoPartyDto? Seller,
        EdoPartyDto? Buyer)
    {
        public static EdocsCommonFields Empty { get; } = new(null, null, null, null, null);
    }

    private static EdoDocumentCategory MapCategory(EdoDirection direction, EdoDocumentStatusCode status) => status switch
    {
        EdoDocumentStatusCode.DRAFT => EdoDocumentCategory.DRAFTS,
        EdoDocumentStatusCode.REJECTED => EdoDocumentCategory.REJECTED,
        EdoDocumentStatusCode.DELETED or EdoDocumentStatusCode.ARCHIVED or EdoDocumentStatusCode.CANCELLED
            => EdoDocumentCategory.DELETED_ARCHIVED,
        _ => direction == EdoDirection.INBOX ? EdoDocumentCategory.INBOX : EdoDocumentCategory.OUTBOX
    };

    private static EdoProviderCode ParseProviderCode(string value) =>
        Enum.TryParse<EdoProviderCode>(value, true, out var result)
            ? result
            : throw new InvalidOperationException("The stored EDO document contains an invalid provider code.");

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
