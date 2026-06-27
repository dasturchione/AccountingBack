namespace Application.Features.PricingConditions;

public class PricingConditionDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
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
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
