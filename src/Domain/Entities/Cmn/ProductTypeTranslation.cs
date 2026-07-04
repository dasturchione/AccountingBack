using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("ProductTypeId", "LanguageId")]
[Table("cmn_product_type_translation")]
public partial class ProductTypeTranslation
{
    [Key]
    [Column("product_type_id")]
    public short ProductTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("ProductTypeTranslations")]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("ProductTypeId")]
    [InverseProperty("ProductTypeTranslations")]
    public virtual ProductType ProductType { get; set; } = null!;
}
