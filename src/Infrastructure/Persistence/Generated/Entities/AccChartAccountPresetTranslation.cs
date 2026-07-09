using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PresetId", "LanguageId")]
[Table("acc_chart_account_preset_translation")]
[Index("LanguageId", Name = "idx_acc_chart_account_preset_translation_language_id")]
public partial class AccChartAccountPresetTranslation
{
    [Key]
    [Column("preset_id")]
    public short PresetId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("AccChartAccountPresetTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PresetId")]
    [InverseProperty("AccChartAccountPresetTranslations")]
    public virtual AccChartAccountPreset Preset { get; set; } = null!;
}
