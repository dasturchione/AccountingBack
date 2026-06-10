namespace Domain.Entities;

public partial class ProductGroup
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? ParentId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual ProductGroup? Parent { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual ICollection<ProductGroup> InverseParent { get; set; } = new List<ProductGroup>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
