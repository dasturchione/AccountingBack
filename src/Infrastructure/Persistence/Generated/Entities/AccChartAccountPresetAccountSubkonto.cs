using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_chart_account_preset_account_subkonto")]
[Index("PresetAccountId", Name = "idx_acc_chart_account_preset_account_subkonto_account_id")]
[Index("SubkontoTypeId", Name = "idx_acc_chart_account_preset_account_subkonto_type_id")]
[Index("PresetAccountId", "SortOrder", Name = "uq_acc_chart_account_preset_account_subkonto_order", IsUnique = true)]
[Index("PresetAccountId", "SubkontoTypeId", Name = "uq_acc_chart_account_preset_account_subkonto_type", IsUnique = true)]
public partial class AccChartAccountPresetAccountSubkonto
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("preset_account_id")]
    public int PresetAccountId { get; set; }

    [Column("subkonto_type_id")]
    public short SubkontoTypeId { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("PresetAccountId")]
    [InverseProperty("AccChartAccountPresetAccountSubkontos")]
    public virtual AccChartAccountPresetAccount PresetAccount { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty("AccChartAccountPresetAccountSubkontos")]
    public virtual AccSubkontoType SubkontoType { get; set; } = null!;
}
