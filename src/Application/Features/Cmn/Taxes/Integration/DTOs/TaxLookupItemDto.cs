namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxLookupItemDto
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IDictionary<string, string?> Metadata { get; init; } = new Dictionary<string, string?>();
}
