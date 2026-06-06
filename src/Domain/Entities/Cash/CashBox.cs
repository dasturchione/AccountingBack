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

    public virtual Organization Organization { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
