using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_depreciation_run_line")]
[Index("FaAssetId", Name = "idx_fa_depreciation_run_line_fa_asset_id")]
[Index("DepreciationRunId", Name = "idx_fa_depreciation_run_line_run_id")]
[Index("AccumulatedDepreciationAccountId", Name = "ix_fa_depr_run_line_accum_depr_account")]
[Index("ExpenseAccountId", Name = "ix_fa_depr_run_line_expense_account")]
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

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [ForeignKey("AccumulatedDepreciationAccountId")]
    [InverseProperty("FaDepreciationRunLineAccumulatedDepreciationAccounts")]
    public virtual AccChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("DepreciationRunId")]
    [InverseProperty("FaDepreciationRunLines")]
    public virtual FaDepreciationRun DepreciationRun { get; set; } = null!;

    [ForeignKey("ExpenseAccountId")]
    [InverseProperty("FaDepreciationRunLineExpenseAccounts")]
    public virtual AccChartAccount? ExpenseAccount { get; set; }

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaDepreciationRunLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;
}
