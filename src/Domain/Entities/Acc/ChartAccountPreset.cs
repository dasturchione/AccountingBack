using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account_preset")]
public partial class ChartAccountPreset
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Preset")]
    public virtual ICollection<ChartAccountPresetAccount> ChartAccountPresetAccounts { get; set; } = new List<ChartAccountPresetAccount>();

    [InverseProperty("Preset")]
    public virtual ICollection<ChartAccountPresetTranslation> ChartAccountPresetTranslations { get; set; } = new List<ChartAccountPresetTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty(nameof(State.ChartAccountPresets))]
    public virtual State State { get; set; } = null!;
}
