namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxDocumentResultDto
{
    public string ProviderCode { get; init; } = null!;
    public string Operation { get; init; } = null!;
    public string? ExternalDocumentId { get; init; }
    public string? StatusCode { get; init; }
    public string? StatusName { get; init; }
    public bool IsSuccessful { get; init; }
    public string? Message { get; init; }
    public DateTime RequestedAt { get; init; }
}
