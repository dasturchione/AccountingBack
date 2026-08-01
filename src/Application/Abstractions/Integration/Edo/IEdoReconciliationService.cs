namespace Application.Abstractions.Integration.Edo;

public interface IEdoReconciliationService
{
    Task<EdoReconciliationResultDto> ReconcileAsync(
        long documentId,
        EdoDirection direction,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<EdoReconciliationCandidateDto>> GetCandidatesAsync(
        EdoDirection? direction = null,
        CancellationToken ct = default);
}

public enum EdoReconciliationState
{
    SKIPPED_NO_PROVIDER_DOCUMENT_ID,
    SYNCHRONIZED,
    PENDING,
    RECONCILIATION_REQUIRED,
    FAILED
}

public sealed class EdoReconciliationResultDto
{
    public long DocumentId { get; init; }
    public EdoProviderCode ProviderCode { get; init; }
    public EdoDirection Direction { get; init; }
    public EdoReconciliationState State { get; init; }
    public bool Attempted { get; init; }
    public bool StatusChanged { get; init; }
    public bool IsRemoteStatusConfirmed { get; init; }
    public EdoDocumentStatusDto Status { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public DateTimeOffset CheckedAt { get; init; }
}

public sealed class EdoReconciliationCandidateDto
{
    public long DocumentId { get; init; }
    public EdoProviderCode ProviderCode { get; init; }
    public EdoDirection Direction { get; init; }
    public EdoDocumentStatusCode LocalStatus { get; init; }
    public string? ProviderDocumentId { get; init; }
    public string? ProviderStatusCode { get; init; }
}
