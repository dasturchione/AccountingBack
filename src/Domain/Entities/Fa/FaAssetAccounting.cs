using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_asset_accounting")]
[Index(nameof(AccumulatedDepreciationAccountId), Name = "ix_fa_asset_accounting_accumulated_depreciation_account_id")]
[Index(nameof(AssetAccountId), Name = "ix_fa_asset_accounting_asset_account_id")]
[Index(nameof(DepreciationExpenseAccountId), Name = "ix_fa_asset_accounting_depreciation_expense_account_id")]
[Index(nameof(DepreciationMethodId), Name = "ix_fa_asset_accounting_depreciation_method_id")]
public partial class FaAssetAccounting
{
    [Key]
    [Column("asset_id")]
    public long AssetId { get; set; }

    [Column("initial_cost")]
    [Precision(24, 8)]
    public decimal InitialCost { get; set; }

    [Column("asset_account_id")]
    public int AssetAccountId { get; set; }

    [Column("salvage_value")]
    [Precision(24, 8)]
    public decimal? SalvageValue { get; set; }

    [Column("depreciation_method_id")]
    public short? DepreciationMethodId { get; set; }

    [Column("useful_life_months")]
    public int? UsefulLifeMonths { get; set; }

    [Column("depr_start_date", TypeName = "timestamp without time zone")]
    public DateTime? DeprStartDate { get; set; }

    [Column("planned_units_total")]
    [Precision(24, 8)]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [Column("depreciation_expense_account_id")]
    public int? DepreciationExpenseAccountId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

    [ForeignKey(nameof(AccumulatedDepreciationAccountId))]
    [InverseProperty(nameof(ChartAccount.FaAssetAccountingAccumulatedDepreciationAccounts))]
    public virtual ChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey(nameof(AssetId))]
    [InverseProperty(nameof(FaAsset.FaAssetAccounting))]
    public virtual FaAsset Asset { get; set; } = null!;

    [ForeignKey(nameof(AssetAccountId))]
    [InverseProperty(nameof(ChartAccount.FaAssetAccountingAssetAccounts))]
    public virtual ChartAccount AssetAccount { get; set; } = null!;

    [ForeignKey(nameof(DepreciationExpenseAccountId))]
    [InverseProperty(nameof(ChartAccount.FaAssetAccountingDepreciationExpenseAccounts))]
    public virtual ChartAccount? DepreciationExpenseAccount { get; set; }

    [ForeignKey(nameof(DepreciationMethodId))]
    [InverseProperty(nameof(FaDepreciationMethod.FaAssetAccountings))]
    public virtual FaDepreciationMethod? DepreciationMethod { get; set; }
}
