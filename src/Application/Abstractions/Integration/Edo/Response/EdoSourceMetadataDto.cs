namespace Application.Abstractions.Integration.Edo;

/// <summary>
/// Safe local EDO source metadata exposed alongside a local document.
/// Provider payloads and marking values are intentionally excluded.
/// </summary>
public sealed class EdoSourceMetadataDto
{
    public long EdoDocumentId { get; init; }
    public string ProviderCode { get; init; } = string.Empty;
    public string? ProviderDocumentId { get; init; }
    public string? DocumentNumber { get; init; }
    public DateOnly? DocumentDate { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
