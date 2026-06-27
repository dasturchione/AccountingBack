namespace Application.Features.SaleConditions;

public class SaleConditionDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public short CostingMethodId { get; set; }
    public string CostingMethodName { get; set; } = null!;
    public string CostingMethodCode { get; set; } = null!;
    public short VatRateId { get; set; }
    public string VatRateName { get; set; } = null!;
    public string VatRateCode { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public short StateId { get; set; }
    public string StateName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
