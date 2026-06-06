namespace Application.Features.CashBoxes;

public class CashBoxBaseDto
{
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short CurrencyId { get; set; }
}
