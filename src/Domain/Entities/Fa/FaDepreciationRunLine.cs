using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_depreciation_run_line")]
[Index("DepreciationRunId", Name = "idx_fa_depreciation_run_line_run_id")]
[Index("FaAssetId", Name = "idx_fa_depreciation_run_line_fa_asset_id")]
[Index("DepreciationRunId", "FaAssetId", Name = "ux_fa_depreciation_run_line_run_asset", IsUnique = true)]
public partial class FaDepreciationRunLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("depreciation_run_id")]
    public long DepreciationRunId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("amount", TypeName = "numeric(18,2)")]
    public decimal Amount { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("DepreciationRunId")]
    [InverseProperty("Lines")]
    public virtual FaDepreciationRun DepreciationRun { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("DepreciationRunLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;
}
