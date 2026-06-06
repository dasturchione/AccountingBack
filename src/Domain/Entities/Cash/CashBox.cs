namespace Domain.Entities;

public partial class CashBox
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short CurrencyId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
}
