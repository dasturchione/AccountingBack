namespace Application.Features.Cmn.Taxes;

public sealed class TaxCalculationResultDto
{
    public int OrganizationId { get; set; }
    public short TaxTypeId { get; set; }
    public string TaxTypeCode { get; set; } = null!;
    public string TaxTypeName { get; set; } = null!;
    public short VatRateId { get; set; }
    public string VatRateCode { get; set; } = null!;
    public string VatRateName { get; set; } = null!;
    public decimal Rate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public TaxCalculationMode CalculationMode { get; set; }
    public short RoundingPrecision { get; set; }
}
