using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("CostingMethodId", "LanguageId")]
[Table("cmn_costing_method_translation")]
[Index("LanguageId", Name = "ix_cmn_costing_method_translation_language_id")]
public partial class CmnCostingMethodTranslation
{
    [Key]
    [Column("costing_method_id")]
    public short CostingMethodId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("CostingMethodId")]
    [InverseProperty("CmnCostingMethodTranslations")]
    public virtual CmnCostingMethod CostingMethod { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnCostingMethodTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
