using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace Application.Features.Integration.Edo;

public sealed class EdoOutboxService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoDocumentStore documentStore,
    IEdoDocumentSigningSessionStore signingSessionStore,
    ILogger<EdoOutboxService> logger) : IEdoOutboxService
{
    private const string CreateOperationType = "FACTURA_CREATE";
    private static readonly TimeSpan DocumentSigningSessionLifetime = TimeSpan.FromMinutes(5);

    public async Task<EdoOutboxCreateDto> CreateFacturaAsync(
        EdoOutboxFacturaCreateRequestDto request,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        EnsureCapability(provider, EdoCapabilityKind.CreateFactura);

        var idempotencyKey = RequireIdempotencyKey(request.IdempotencyKey);
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
            await documentStore.AddAsync(document, ct);
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
            await MarkCreateFailureAsync(document.Id, organizationId, provider.Code, providerScopedKey, null, ex);
            throw;
        }

        try
        {
            if (providerResult.Document.LegacyDocumentId is null
                || providerResult.Document.LegacyDocumentId <= 0
                || string.IsNullOrWhiteSpace(providerResult.Document.ProviderDocumentId))
            {
                throw new InvalidOperationException("The provider did not return a complete factura document identity.");
            }

            document = await documentStore.GetAsync(organizationId, provider.Code, document.Id, CancellationToken.None)
                ?? throw new InvalidOperationException("The pending EDO document disappeared before result persistence.");

            ApplyProviderResult(document, providerResult.Document);
            await documentStore.UpdateAsync(document, CancellationToken.None);
        }
        catch (Exception ex)
        {
            await MarkCreateFailureAsync(
                document.Id,
                organizationId,
                provider.Code,
                providerScopedKey,
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
            || document.LegacyDocumentId is null
            || string.IsNullOrWhiteSpace(document.ProviderDocumentId))
        {
            throw new InvalidOperationException("The EDO document is not ready for signing.");
        }

        if (document.Status is nameof(EdoDocumentStatusCode.FAILED)
            or nameof(EdoDocumentStatusCode.RECONCILIATION_REQUIRED))
        {
            throw new InvalidOperationException("The EDO document requires reconciliation before signing.");
        }

        var isDidoxChallengeRequest = provider.Code == EdoProviderCode.DIDOX
            && string.IsNullOrWhiteSpace(request.SigningSessionId)
            && string.IsNullOrWhiteSpace(request.PreparedPkcs7)
            && string.IsNullOrWhiteSpace(request.SignatureHex);

        if (isDidoxChallengeRequest)
        {
            var challenge = await provider.SignOutboxAsync(document.LegacyDocumentId.Value, request, ct);
            var providerSession = challenge.SigningSession
                ?? throw new EdoCapabilityUnavailableException(
                    provider.Code.ToString(),
                    "SignChallenge",
                    EdoCapabilityStatus.UNKNOWN.ToString());

            var session = await signingSessionStore.CreateAsync(
                organizationId,
                provider.Code,
                document.Id,
                providerSession.SigningMode,
                providerSession.ExpiresAt ?? DateTimeOffset.UtcNow.Add(DocumentSigningSessionLifetime),
                ct);

            return new EdoOutboxSignDto
            {
                Document = MapDocument(document),
                SigningSession = new EdoSigningSessionDto
                {
                    SessionId = session.SessionId,
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
            await signingSessionStore.ConsumeAsync(
                organizationId,
                provider.Code,
                document.Id,
                request.SigningSessionId,
                ct);
        }

        var providerResult = await provider.SignOutboxAsync(document.LegacyDocumentId.Value, request, ct);

        try
        {
            document = await documentStore.GetAsync(organizationId, provider.Code, id, CancellationToken.None)
                ?? throw new InvalidOperationException("The EDO document disappeared during signing.");
            ApplyProviderResult(document, providerResult.Document);
            await documentStore.UpdateAsync(document, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "EDO sign succeeded remotely but common EdoDocument {DocumentId} local status update failed. Manual reconciliation required.",
                id);
            throw;
        }

        return new EdoOutboxSignDto { Document = MapDocument(document) };
    }

    private async Task MarkCreateFailureAsync(
        long documentId,
        int organizationId,
        EdoProviderCode providerCode,
        string providerScopedKey,
        string? fallbackProviderDocumentId,
        Exception originalException)
    {
        try
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
                return;

            document.ProviderDocumentId = providerDocumentId;
            document.Status = string.IsNullOrWhiteSpace(providerDocumentId)
                ? EdoDocumentStatusCode.FAILED.ToString()
                : EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString();
            document.ErrorMessage = string.IsNullOrWhiteSpace(providerDocumentId)
                ? "EDO factura creation failed."
                : "Remote factura creation succeeded but local reconciliation is required.";
            document.UpdatedAt = DateTime.UtcNow;
            await documentStore.UpdateAsync(document, CancellationToken.None);

            if (!string.IsNullOrWhiteSpace(providerDocumentId))
            {
                logger.LogCritical(
                    originalException,
                    "EDO factura creation succeeded remotely with ProviderDocumentId {ProviderDocumentId}, but common local persistence requires reconciliation for EdoDocument {DocumentId}.",
                    providerDocumentId,
                    documentId);
            }
        }
        catch (Exception reconciliationException)
        {
            logger.LogCritical(
                reconciliationException,
                "Failed to persist EDO factura reconciliation state for EdoDocument {DocumentId}.",
                documentId);
        }
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
        return $"EDO_{providerCode}_FACTURA_{hash}";
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

    private static string RequireIdempotencyKey(string key) =>
        string.IsNullOrWhiteSpace(key)
            ? throw new InvalidOperationException("IdempotencyKey is required for EDO factura creation.")
            : key.Length > 200
                ? throw new InvalidOperationException("IdempotencyKey must not exceed 200 characters.")
                : key;

    private static string RequireText(string value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is required for EDO factura creation.")
            : value;

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new EdoOrganizationScopeRequiredException();
}
