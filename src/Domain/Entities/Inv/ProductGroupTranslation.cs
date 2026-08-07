using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("ProductGroupId", "LanguageId")]
[Table("inv_product_group_translation")]
public partial class ProductGroupTranslation
{
    [Key]
    [Column("product_group_id")]
    public int ProductGroupId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.ProductGroupTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("ProductGroupId")]
    [InverseProperty(nameof(ProductGroup.ProductGroupTranslations))]
    public virtual ProductGroup ProductGroup { get; set; } = null!;
}
