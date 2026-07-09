using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("PresetId", "LanguageId")]
[Table("acc_chart_account_preset_translation")]
public partial class ChartAccountPresetTranslation
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
    [InverseProperty(nameof(Language.ChartAccountPresetTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PresetId")]
    [InverseProperty(nameof(ChartAccountPreset.ChartAccountPresetTranslations))]
    public virtual ChartAccountPreset Preset { get; set; } = null!;
}
