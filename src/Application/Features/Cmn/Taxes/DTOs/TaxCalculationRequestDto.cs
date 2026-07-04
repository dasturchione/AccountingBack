namespace Application.Features.Cmn.Taxes;

public sealed class TaxCalculationRequestDto
{
    public short TaxTypeId { get; set; }
    public decimal Amount { get; set; }
    public TaxCalculationMode CalculationMode { get; set; }
    public DateOnly? EffectiveDate { get; set; }
    public int? OrganizationId { get; set; }
    public short? RoundingPrecision { get; set; }
}
