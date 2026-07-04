using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("ProductTypeId", "LanguageId")]
[Table("cmn_product_type_translation")]
public partial class CmnProductTypeTranslation
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
    [InverseProperty("CmnProductTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("ProductTypeId")]
    [InverseProperty("CmnProductTypeTranslations")]
    public virtual CmnProductType ProductType { get; set; } = null!;
}
