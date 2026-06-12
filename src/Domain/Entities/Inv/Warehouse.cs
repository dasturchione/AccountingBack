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
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<PurchaseDoc> PurDocs { get; set; } = new List<PurchaseDoc>();
    public virtual User? ResponsibleUser { get; set; }
    public virtual ICollection<SaleDoc> SaleDocs { get; set; } = new List<SaleDoc>();
    public virtual State State { get; set; } = null!;
}
