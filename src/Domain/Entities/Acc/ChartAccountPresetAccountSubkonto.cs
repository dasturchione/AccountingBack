using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_chart_account_preset_account_subkonto")]
public partial class ChartAccountPresetAccountSubkonto
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
    [InverseProperty("ChartAccountPresetAccountSubkontos")]
    public virtual ChartAccountPresetAccount PresetAccount { get; set; } = null!;

    [ForeignKey("SubkontoTypeId")]
    [InverseProperty(nameof(SubkontoType.ChartAccountPresetAccountSubkontos))]
    public virtual SubkontoType SubkontoType { get; set; } = null!;
}
