namespace Application.Features.Cmn.Taxes.Integration.DTOs;

public sealed class TaxDocumentRequestDto
{
    public string ProviderCode { get; init; } = null!;
    public int? OrganizationId { get; init; }
    public string? DocumentNumber { get; init; }
    public string? Payload { get; init; }
    public string? ExternalDocumentId { get; init; }
}
