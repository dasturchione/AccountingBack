using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Integration.Edo;
using Application.Features;
using Application.Features.AuditLogs;
using Domain.Entities;
using Integration.Edo.Providers;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Results;

namespace Integration.Edo.Reconciliation;

/// <summary>
/// Performs status-only synchronization after an operation may have left the
/// local and provider states out of sync. It never retries a remote write.
/// </summary>
public sealed class EdoReconciliationService(
    IUserContext userContext,
    IActiveEdoProviderResolver activeProviderResolver,
    IEdoDocumentStore documentStore,
    ILogger<EdoReconciliationService> logger,
    IAuditLogService auditLogService,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IEdoReconciliationService
{
    private const string DocumentTable = "edo_document";

    public async Task<EdoReconciliationResultDto> ReconcileAsync(
        long documentId,
        EdoDirection direction,
        CancellationToken ct = default)
    {
        if (documentId <= 0)
            throw new EdoDocumentNotFoundException();

        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var document = await documentStore.GetAsync(organizationId, provider.Code, documentId, ct)
            ?? throw new EdoDocumentNotFoundException();

        if (!string.Equals(document.Direction, direction.ToString(), StringComparison.Ordinal))
            throw new EdoDocumentDirectionMismatchException();

        var localStatus = ParseStatus(document.Status);
        if (string.IsNullOrWhiteSpace(document.ProviderDocumentId))
        {
            return new EdoReconciliationResultDto
            {
                DocumentId = document.Id,
                ProviderCode = provider.Code,
                Direction = direction,
                State = EdoReconciliationState.SKIPPED_NO_PROVIDER_DOCUMENT_ID,
                Status = CreateStatus(localStatus, localStatus, document.ProviderStatusCode,
                    isReconciliationRequired: localStatus == EdoDocumentStatusCode.RECONCILIATION_REQUIRED),
                CheckedAt = DateTimeOffset.UtcNow,
                ErrorMessage = "Reconciliation was skipped because the provider document ID is missing."
            };
        }

        EnsureStatusCapability(provider, direction);

        EdoDocumentStatusDto providerStatus;
        try
        {
            providerStatus = direction == EdoDirection.OUTBOX
                ? await provider.GetOutboxStatusAsync(document.DocumentType, document.ProviderDocumentId, ct)
                : await provider.GetInboxStatusAsync(document.DocumentType, document.ProviderDocumentId, ct);
            providerStatus = MapProviderStatus(provider.Code, providerStatus);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return await MarkUnknownOutcomeAsync(
                document,
                provider.Code,
                direction,
                localStatus,
                "The provider status request timed out; the remote outcome is unknown.",
                ct);
        }
        catch (HttpRequestException ex)
        {
            return await MarkUnknownOutcomeAsync(
                document,
                provider.Code,
                direction,
                localStatus,
                "The provider status request failed; the remote outcome is unknown.",
                CancellationToken.None,
                ex);
        }
        catch (IntegrationHttpException ex) when (ex.StatusCode is 408 or 429 or >= 500)
        {
            return await MarkUnknownOutcomeAsync(
                document,
                provider.Code,
                direction,
                localStatus,
                "The provider status request failed; the remote outcome is unknown.",
                CancellationToken.None,
                ex);
        }

        var checkedAt = DateTimeOffset.UtcNow;
        var statusIsKnown = providerStatus.Code != EdoDocumentStatusCode.UNKNOWN;
        var statusRequiresReconciliation = providerStatus.Code == EdoDocumentStatusCode.RECONCILIATION_REQUIRED;

        if (!statusIsKnown)
        {
            var oldStatus = document.Status;
            document.ProviderStatusCode = providerStatus.ProviderStatusCode;
            document.UpdatedAt = DateTime.UtcNow;
            await PersistDiagnosticsAsync(document, document.Id, provider.Code, oldStatus, ct);

            return new EdoReconciliationResultDto
            {
                DocumentId = document.Id,
                ProviderCode = provider.Code,
                Direction = direction,
                State = EdoReconciliationState.RECONCILIATION_REQUIRED,
                Attempted = true,
                IsRemoteStatusConfirmed = true,
                Status = CreateStatus(localStatus, EdoDocumentStatusCode.UNKNOWN,
                    providerStatus.ProviderStatusCode, isReconciliationRequired: true),
                CheckedAt = checkedAt,
                ErrorMessage = "The provider returned a status that is not mapped to the common EDO status model."
            };
        }

        var statusChanged = localStatus != providerStatus.Code;
        if (statusChanged)
        {
            logger.LogInformation(
                "EDO reconciliation changed document {DocumentId} status: local={LocalStatus}, provider={ProviderStatus}.",
                document.Id,
                localStatus,
                providerStatus.ProviderStatusCode ?? providerStatus.Code.ToString());
        }

        document.Status = providerStatus.Code.ToString();
        document.ProviderStatusCode = providerStatus.ProviderStatusCode;
        document.ErrorMessage = null;
        document.UpdatedAt = DateTime.UtcNow;
        await PersistDiagnosticsAsync(document, document.Id, provider.Code, localStatus.ToString(), ct);

        return new EdoReconciliationResultDto
        {
            DocumentId = document.Id,
            ProviderCode = provider.Code,
            Direction = direction,
            State = statusRequiresReconciliation
                ? EdoReconciliationState.RECONCILIATION_REQUIRED
                : providerStatus.Code is EdoDocumentStatusCode.PENDING or EdoDocumentStatusCode.SENT
                    ? EdoReconciliationState.PENDING
                    : EdoReconciliationState.SYNCHRONIZED,
            Attempted = true,
            StatusChanged = statusChanged,
            IsRemoteStatusConfirmed = true,
            Status = CreateStatus(localStatus, providerStatus.Code,
                providerStatus.ProviderStatusCode, providerStatus.Description,
                providerStatus.IsTerminal, providerStatus.IsSuccessful,
                statusRequiresReconciliation),
            CheckedAt = checkedAt
        };
    }

    public async Task<IReadOnlyCollection<EdoReconciliationCandidateDto>> GetCandidatesAsync(
        EdoDirection? direction = null,
        CancellationToken ct = default)
    {
        var organizationId = RequireOrganization();
        var provider = await activeProviderResolver.GetActiveProviderAsync(ct);
        var documents = await documentStore.GetReconciliationCandidatesAsync(
            organizationId, provider.Code, direction, ct);

        return documents.Select(document => new EdoReconciliationCandidateDto
        {
            DocumentId = document.Id,
            ProviderCode = provider.Code,
            Direction = ParseDirection(document.Direction),
            LocalStatus = ParseStatus(document.Status),
            ProviderDocumentId = document.ProviderDocumentId,
            ProviderStatusCode = document.ProviderStatusCode
        }).ToList();
    }

    private async Task<EdoReconciliationResultDto> MarkUnknownOutcomeAsync(
        EdoDocument document,
        EdoProviderCode providerCode,
        EdoDirection direction,
        EdoDocumentStatusCode localStatus,
        string message,
        CancellationToken ct,
        Exception? exception = null)
    {
        var oldStatus = document.Status;
        document.Status = EdoDocumentStatusCode.RECONCILIATION_REQUIRED.ToString();
        document.ErrorMessage = message;
        document.UpdatedAt = DateTime.UtcNow;
        try
        {
            await PersistDiagnosticsAsync(document, document.Id, providerCode, oldStatus, ct);
        }
        catch (Exception persistenceException)
        {
            logger.LogCritical(
                persistenceException,
                "Failed to persist EDO reconciliation state for document {DocumentId}.",
                document.Id);
            throw;
        }

        logger.LogError(
            exception,
            "EDO provider status is unknown for document {DocumentId}; reconciliation is required.",
            document.Id);

        return new EdoReconciliationResultDto
        {
            DocumentId = document.Id,
            ProviderCode = providerCode,
            Direction = direction,
            State = EdoReconciliationState.FAILED,
            Attempted = true,
            IsRemoteStatusConfirmed = false,
            Status = CreateStatus(localStatus, EdoDocumentStatusCode.RECONCILIATION_REQUIRED,
                document.ProviderStatusCode, isReconciliationRequired: true),
            CheckedAt = DateTimeOffset.UtcNow,
            ErrorMessage = message
        };
    }

    private async Task PersistDiagnosticsAsync(
        EdoDocument document,
        long documentId,
        EdoProviderCode providerCode,
        string oldStatus,
        CancellationToken ct)
    {
        try
        {
            await ExecuteInTransactionAsync("PersistReconciliationStatus", async () =>
            {
                await documentStore.UpdateAsync(document, ct);
                auditLogService.SetOldValues(new
                {
                    operation = "RECONCILIATION_STATUS_UPDATE",
                    organizationId = document.OrganizationId,
                    providerCode = providerCode.ToString(),
                    documentId = document.Id,
                    status = oldStatus
                });
                auditLogService.SetNewValues(new
                {
                    operation = "RECONCILIATION_STATUS_UPDATE",
                    organizationId = document.OrganizationId,
                    providerCode = providerCode.ToString(),
                    documentId = document.Id,
                    status = document.Status
                });
                await auditLogService.CreateAsync(
                    DocumentTable,
                    document.Id.ToString(),
                    AuditLogOperationTypeConst.Update);
                return Result.Success();
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "EDO reconciliation local persistence failed for document {DocumentId}.", documentId);
            throw;
        }
    }

    private static EdoDocumentStatusDto MapProviderStatus(
        EdoProviderCode providerCode,
        EdoDocumentStatusDto status)
    {
        if (string.IsNullOrWhiteSpace(status.ProviderStatusCode))
            return status;

        return providerCode switch
        {
            EdoProviderCode.DIDOX when int.TryParse(status.ProviderStatusCode, out var didoxStatus)
                => EdoProviderStatusMapper.MapDidoxStatus(didoxStatus),
            EdoProviderCode.EDOCS => EdoProviderStatusMapper.MapEdocsStatus(status.ProviderStatusCode),
            EdoProviderCode.FAKTURA => status,
            _ => EdoProviderStatusMapper.Map(status.ProviderStatusCode)
        };
    }

    private static void EnsureStatusCapability(IEdoProvider provider, EdoDirection direction)
    {
        var capability = direction == EdoDirection.OUTBOX
            ? EdoCapabilityKind.GetOutboxStatus
            : EdoCapabilityKind.GetInboxStatus;
        var status = provider.Capabilities.Capabilities
            .SingleOrDefault(item => item.Kind == capability)?.Status
            ?? EdoCapabilityStatus.UNKNOWN;
        if (status != EdoCapabilityStatus.SUPPORTED)
            throw new EdoCapabilityUnavailableException(
                provider.Code.ToString(), capability.ToString(), status.ToString());
    }

    private int RequireOrganization() => userContext.OrganizationId
        ?? throw new EdoOrganizationScopeRequiredException();

    private static EdoDocumentStatusDto CreateStatus(
        EdoDocumentStatusCode localStatus,
        EdoDocumentStatusCode providerStatus,
        string? providerStatusCode,
        string? description = null,
        bool isTerminal = false,
        bool isSuccessful = false,
        bool isReconciliationRequired = false) => new()
        {
            Code = providerStatus,
            LocalCode = localStatus,
            ProviderStatusCode = providerStatusCode,
            Description = description,
            IsTerminal = isTerminal,
            IsSuccessful = isSuccessful,
            CheckedAt = DateTimeOffset.UtcNow,
            IsReconciliationRequired = isReconciliationRequired
                || providerStatus == EdoDocumentStatusCode.RECONCILIATION_REQUIRED
        };

    private static EdoDirection ParseDirection(string value) =>
        Enum.TryParse<EdoDirection>(value, true, out var result) ? result : EdoDirection.INBOX;

    private static EdoDocumentStatusCode ParseStatus(string value) =>
        Enum.TryParse<EdoDocumentStatusCode>(value, true, out var result) ? result : EdoDocumentStatusCode.UNKNOWN;
}
