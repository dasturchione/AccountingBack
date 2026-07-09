using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PresetAccountId", "LanguageId")]
[Table("acc_chart_account_preset_account_translation")]
[Index("LanguageId", Name = "idx_acc_chart_account_preset_account_translation_language_id")]
public partial class AccChartAccountPresetAccountTranslation
{
    [Key]
    [Column("preset_account_id")]
    public int PresetAccountId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccChartAccountPresetAccountTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PresetAccountId")]
    [InverseProperty("AccChartAccountPresetAccountTranslations")]
    public virtual AccChartAccountPresetAccount PresetAccount { get; set; } = null!;
}
