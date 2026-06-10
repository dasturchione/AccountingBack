namespace Domain.Entities;

public partial class Warehouse
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? ResponsibleUserId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual User? ResponsibleUser { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<InventoryRegisterBalance> InventoryRegisterBalances { get; set; } = new List<InventoryRegisterBalance>();
    public virtual ICollection<PurchaseDoc> PurchaseDocs { get; set; } = new List<PurchaseDoc>();
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();
}
