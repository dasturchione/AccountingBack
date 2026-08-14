using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_asset_accounting")]
[Index("AccumulatedDepreciationAccountId", Name = "ix_fa_asset_accounting_accumulated_depreciation_account_id")]
[Index("AssetAccountId", Name = "ix_fa_asset_accounting_asset_account_id")]
[Index("DepreciationExpenseAccountId", Name = "ix_fa_asset_accounting_depreciation_expense_account_id")]
[Index("DepreciationMethodId", Name = "ix_fa_asset_accounting_depreciation_method_id")]
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

    [ForeignKey("AccumulatedDepreciationAccountId")]
    [InverseProperty("FaAssetAccountingAccumulatedDepreciationAccounts")]
    public virtual AccChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("AssetId")]
    [InverseProperty("FaAssetAccounting")]
    public virtual FaAsset Asset { get; set; } = null!;

    [ForeignKey("AssetAccountId")]
    [InverseProperty("FaAssetAccountingAssetAccounts")]
    public virtual AccChartAccount AssetAccount { get; set; } = null!;

    [ForeignKey("DepreciationExpenseAccountId")]
    [InverseProperty("FaAssetAccountingDepreciationExpenseAccounts")]
    public virtual AccChartAccount? DepreciationExpenseAccount { get; set; }

    [ForeignKey("DepreciationMethodId")]
    [InverseProperty("FaAssetAccountings")]
    public virtual CmnFaDepreciationMethod? DepreciationMethod { get; set; }
}
