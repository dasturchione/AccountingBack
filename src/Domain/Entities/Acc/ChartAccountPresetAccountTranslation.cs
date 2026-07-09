using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("PresetAccountId", "LanguageId")]
[Table("acc_chart_account_preset_account_translation")]
public partial class ChartAccountPresetAccountTranslation
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
    [InverseProperty(nameof(Language.ChartAccountPresetAccountTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PresetAccountId")]
    [InverseProperty(nameof(ChartAccountPresetAccount.ChartAccountPresetAccountTranslations))]
    public virtual ChartAccountPresetAccount PresetAccount { get; set; } = null!;
}
