using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using System.Globalization;

namespace Application.Features.Integration.Edo;

public sealed class EdoInboxService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoDocumentStore documentStore,
    IEdoDocumentSigningSessionStore signingSessionStore,
    IEdoReconciliationService reconciliationService,
    ILogger<EdoInboxService> logger) : IEdoInboxService
{
    private const string InboxSyncOperation = "INBOX_SYNC";
    private const string RejectOperation = "INBOX_REJECT";
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
        foreach (var providerDocument in providerResult.Items)
        {
            var localDocument = await UpsertInboxDocumentAsync(organizationId, provider.Code, providerDocument, ct);
            items.Add(MapDocument(localDocument, providerDocument));
        }

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

        if (IsTerminal(document.Status))
            throw new InvalidOperationException("A terminal EDO inbox document cannot be rejected again.");

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

            if (existing.Status != EdoDocumentStatusCode.PENDING.ToString())
                return new EdoInboxRejectDto { Document = MapDocument(existing) };

            document = existing;
        }
        else
        {
            document.OperationType = RejectOperation;
            document.IdempotencyKey = scopedKey;
            document.RejectReason = reason;
            document.Status = EdoDocumentStatusCode.PENDING.ToString();
            document.ErrorMessage = null;
            document.UpdatedAt = DateTime.UtcNow;
            await documentStore.UpdateAsync(document, ct);
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
                    document.ProviderDocumentId!,
                    CopyRejectRequest(request, reason),
                    ct);
            }
            catch (Exception ex)
            {
                await MarkRejectFailureAsync(document, ex, ct);
                throw;
            }
            var providerSession = challenge.SigningSession
                ?? throw new EdoCapabilityUnavailableException(
                    provider.Code.ToString(),
                    EdoCapabilityKind.RejectInbox.ToString(),
                    EdoCapabilityStatus.UNKNOWN.ToString());

            var session = await signingSessionStore.CreateAsync(
                organizationId,
                provider.Code,
                document.Id,
                providerSession.SigningMode,
                providerSession.ExpiresAt ?? DateTimeOffset.UtcNow.Add(SigningSessionLifetime),
                ct);

            return new EdoInboxRejectDto
            {
                Document = MapDocument(document),
                SigningSession = new EdoSigningSessionDto
                {
                    SessionId = session.SessionId,
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

        if (provider.Code == EdoProviderCode.EDOCS
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7))
            throw new InvalidOperationException("Edocs inbox rejection requires PreparedPkcs7.");

        if (!string.IsNullOrWhiteSpace(request.SigningSessionId))
        {
            await signingSessionStore.ConsumeAsync(
                organizationId,
                provider.Code,
                document.Id,
                request.SigningSessionId,
                ct);
        }

        EdoInboxRejectDto providerResult;
        try
        {
            providerResult = await provider.RejectInboxAsync(document.ProviderDocumentId!, CopyRejectRequest(request, reason), ct);
        }
        catch (Exception ex)
        {
            await MarkRejectFailureAsync(document, ex, ct);
            throw;
        }

        try
        {
            document = await documentStore.GetAsync(organizationId, provider.Code, id, CancellationToken.None)
                ?? throw new InvalidOperationException("The EDO inbox document disappeared during rejection.");
            document.Status = providerResult.Document.Status.Code.ToString();
            document.ProviderStatusCode = providerResult.Document.Status.ProviderStatusCode;
            document.UpdatedAt = DateTime.UtcNow;
            await documentStore.UpdateAsync(document, CancellationToken.None);
        }
        catch (Exception ex)
        {
            document.Status = EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString();
            document.ErrorMessage = "Remote reject succeeded but local status persistence requires reconciliation.";
            document.UpdatedAt = DateTime.UtcNow;
            try { await documentStore.UpdateAsync(document, CancellationToken.None); }
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
            file = await provider.GetFileAsync(document.ProviderDocumentId!, ct);
        }
        catch (Exception ex)
        {
            document.ErrorMessage = "EDO file download failed; reconciliation or a later manual download may be required.";
            document.UpdatedAt = DateTime.UtcNow;
            try { await documentStore.UpdateAsync(document, CancellationToken.None); }
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
            await documentStore.UpdateAsync(document, ct);
        }

        return document;
    }

    private async Task MarkRejectFailureAsync(EdoDocument document, Exception exception, CancellationToken ct)
    {
        var remoteStateUnknown = exception is OperationCanceledException
            or HttpRequestException
            or IntegrationHttpException { StatusCode: 408 or 429 or >= 500 };
        document.Status = remoteStateUnknown
            ? EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString()
            : EdoDocumentStatusCode.FAILED.ToString();
        document.ErrorMessage = remoteStateUnknown
            ? "EDO reject outcome is unknown after a transport failure; reconciliation is required."
            : exception.Message;
        document.UpdatedAt = DateTime.UtcNow;
        try { await documentStore.UpdateAsync(document, ct); }
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

    private static EdoDirection ParseDirection(string value) =>
        Enum.TryParse<EdoDirection>(value, true, out var result) ? result : EdoDirection.INBOX;

    private static EdoDocumentStatusCode ParseStatus(string value) =>
        Enum.TryParse<EdoDocumentStatusCode>(value, true, out var result) ? result : EdoDocumentStatusCode.UNKNOWN;
}
