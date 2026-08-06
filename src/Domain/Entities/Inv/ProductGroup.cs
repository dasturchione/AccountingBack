using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_group")]
public partial class ProductGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("is_assignable")]
    public bool IsAssignable { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [InverseProperty(nameof(Product.ProductGroup))]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty(nameof(ProductGroupTranslation.ProductGroup))]
    public virtual ICollection<ProductGroupTranslation> ProductGroupTranslations { get; set; } = new List<ProductGroupTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.ProductGroups))]
    public virtual State State { get; set; } = null!;
}
