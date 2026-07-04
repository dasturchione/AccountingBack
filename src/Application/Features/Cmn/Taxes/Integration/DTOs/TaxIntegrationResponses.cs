namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxLookupItemDto
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();
}

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
