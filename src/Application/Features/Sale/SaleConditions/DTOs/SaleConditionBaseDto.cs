namespace Application.Features.SaleConditions;

public class SaleConditionBaseDto
{
    public short CostingMethodId { get; set; }
    public short VatRateId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
