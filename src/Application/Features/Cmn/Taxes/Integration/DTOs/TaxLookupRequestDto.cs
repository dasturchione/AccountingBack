namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxLookupRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public string? Query { get; init; }
    public int? OrganizationId { get; init; }
    public DateOnly? EffectiveDate { get; init; }
}
