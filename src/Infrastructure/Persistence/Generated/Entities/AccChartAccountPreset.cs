using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account_preset")]
[Index("Code", Name = "acc_chart_account_preset_code_key", IsUnique = true)]
public partial class AccChartAccountPreset
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
    public virtual ICollection<AccChartAccountPresetAccount> AccChartAccountPresetAccounts { get; set; } = new List<AccChartAccountPresetAccount>();

    [InverseProperty("Preset")]
    public virtual ICollection<AccChartAccountPresetTranslation> AccChartAccountPresetTranslations { get; set; } = new List<AccChartAccountPresetTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("AccChartAccountPresets")]
    public virtual CmnState State { get; set; } = null!;
}
