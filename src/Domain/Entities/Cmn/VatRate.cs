namespace Domain.Entities;

public partial class VatRate
{
    public short Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Rate { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<PurchaseDocTable> PurchaseDocTables { get; set; } = new List<PurchaseDocTable>();
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();
}
