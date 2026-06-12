namespace Domain.Entities;

public partial class ProductTable
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string? Barcode { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();
    public virtual State State { get; set; } = null!;
}
