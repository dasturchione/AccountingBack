namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxLookupRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public string? Query { get; init; }
    public int? OrganizationId { get; init; }
    public DateOnly? EffectiveDate { get; init; }
}

public sealed class TaxDocumentRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? Payload { get; init; }
    public string? ExternalDocumentId { get; init; }

}
