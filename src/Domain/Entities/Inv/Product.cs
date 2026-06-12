namespace Domain.Entities;

public partial class Product
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? ProductGroupId { get; set; }
    public short UnitId { get; set; }
    public string Code { get; set; } = null!;
    public string? Barcode { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsService { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
    public virtual ICollection<ProductTable> ProductTables { get; set; } = new List<ProductTable>();
    public virtual ICollection<RegisterBalance> RegisterBalances { get; set; } = new List<RegisterBalance>();
    public virtual Organization Organization { get; set; } = null!;
    public virtual ProductGroup? ProductGroup { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual Unit Unit { get; set; } = null!;
}
