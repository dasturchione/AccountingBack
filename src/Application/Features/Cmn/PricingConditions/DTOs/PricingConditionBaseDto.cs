namespace Application.Features.PricingConditions;

public class PricingConditionBaseDto
{
    public short PricingMethodId { get; set; }
    public decimal PricingValue { get; set; }
    public short RoundingMethodId { get; set; }
    public decimal RoundingPrecision { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
