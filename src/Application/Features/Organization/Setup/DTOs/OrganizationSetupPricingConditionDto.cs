namespace Application.Features.OrganizationSetup;

public sealed class OrganizationSetupPricingConditionDto
{
    public long Id { get; set; }
    public short PricingMethodId { get; set; }
    public string PricingMethodName { get; set; } = null!;
    public string PricingMethodCode { get; set; } = null!;
    public decimal PricingValue { get; set; }
    public short RoundingMethodId { get; set; }
    public string RoundingMethodName { get; set; } = null!;
    public string RoundingMethodCode { get; set; } = null!;
    public decimal RoundingPrecision { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
