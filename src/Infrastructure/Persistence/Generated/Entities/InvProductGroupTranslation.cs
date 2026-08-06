using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("ProductGroupId", "LanguageId")]
[Table("inv_product_group_translation")]
[Index("LanguageId", Name = "ix_inv_product_group_translation_language_id")]
public partial class InvProductGroupTranslation
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
    [InverseProperty("InvProductGroupTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("ProductGroupId")]
    [InverseProperty("InvProductGroupTranslations")]
    public virtual InvProductGroup ProductGroup { get; set; } = null!;
}
