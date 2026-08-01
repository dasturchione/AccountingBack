using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_depreciation_run_line")]
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

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [ForeignKey(nameof(ExpenseAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDepreciationRunLineExpenseAccounts))]
    public virtual ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey(nameof(AccumulatedDepreciationAccountId))]
    [InverseProperty(nameof(ChartAccount.FaDepreciationRunLineAccumulatedDepreciationAccounts))]
    public virtual ChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("DepreciationRunId")]
    [InverseProperty("Lines")]
    public virtual FaDepreciationRun DepreciationRun { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("DepreciationRunLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;
}
