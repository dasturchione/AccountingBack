using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_group")]
[Index("OrganizationId", Name = "idx_inv_product_group_organization_id")]
[Index("ParentId", Name = "idx_inv_product_group_parent_id")]
[Index("StateId", Name = "idx_inv_product_group_state_id")]
public partial class ProductGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("ProductGroup")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("Parent")]
    public virtual ICollection<ProductGroup> InverseParent { get; set; } = new List<ProductGroup>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("ProductGroups")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual ProductGroup? Parent { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("ProductGroups")]
    public virtual State State { get; set; } = null!;
}
