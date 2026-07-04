namespace Application.Features.Cmn.Taxes;

public sealed class TaxResolutionResultDto
{
    public int OrganizationId { get; set; }
    public short TaxTypeId { get; set; }
    public string TaxTypeCode { get; set; } = null!;
    public string TaxTypeName { get; set; } = null!;
    public short VatRateId { get; set; }
    public string VatRateCode { get; set; } = null!;
    public string VatRateName { get; set; } = null!;
    public decimal Rate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public short StateId { get; set; }
}
