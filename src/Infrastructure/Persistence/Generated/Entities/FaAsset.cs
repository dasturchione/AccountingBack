using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_asset")]
[Index("CommissioningDate", Name = "idx_fa_asset_commissioning_date")]
[Index("DepartmentId", Name = "idx_fa_asset_department_id")]
[Index("DepreciationMethodId", Name = "idx_fa_asset_depreciation_method_id")]
[Index("FaGroupId", Name = "idx_fa_asset_fa_group_id")]
[Index("OkofId", Name = "idx_fa_asset_okof_id")]
[Index("ResponsibleUserId", Name = "idx_fa_asset_responsible_user_id")]
[Index("StateId", Name = "idx_fa_asset_state_id")]
[Index("StatusId", Name = "idx_fa_asset_status_id")]
[Index("AccumulatedDepreciationAccountId", Name = "ix_fa_asset_accum_depr_account")]
[Index("AssetAccountId", Name = "ix_fa_asset_asset_account")]
[Index("DepreciationExpenseAccountId", Name = "ix_fa_asset_depr_exp_account")]
[Index("OrganizationId", "InventoryNumber", Name = "uidx_fa_asset_org_inventory_number", IsUnique = true)]
public partial class FaAsset
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("inventory_number")]
    [StringLength(100)]
    public string InventoryNumber { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("fa_group_id")]
    public int FaGroupId { get; set; }

    [Column("okof_id")]
    public short? OkofId { get; set; }

    [Column("depreciation_method_id")]
    public short DepreciationMethodId { get; set; }

    [Column("useful_life_months")]
    public int UsefulLifeMonths { get; set; }

    [Column("initial_cost")]
    [Precision(24, 8)]
    public decimal InitialCost { get; set; }

    [Column("salvage_value")]
    [Precision(24, 8)]
    public decimal SalvageValue { get; set; }

    [Column("commissioning_date", TypeName = "timestamp without time zone")]
    public DateTime? CommissioningDate { get; set; }

    [Column("depr_start_date", TypeName = "timestamp without time zone")]
    public DateTime? DeprStartDate { get; set; }

    [Column("planned_units_total")]
    [Precision(18, 3)]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime UpdatedDate { get; set; }

    [Column("asset_account_id")]
    public int? AssetAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [Column("depreciation_expense_account_id")]
    public int? DepreciationExpenseAccountId { get; set; }

    [ForeignKey("AccumulatedDepreciationAccountId")]
    [InverseProperty("FaAssetAccumulatedDepreciationAccounts")]
    public virtual AccChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey("AssetAccountId")]
    [InverseProperty("FaAssetAssetAccounts")]
    public virtual AccChartAccount? AssetAccount { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("FaAssets")]
    public virtual OrgDepartment? Department { get; set; }

    [ForeignKey("DepreciationExpenseAccountId")]
    [InverseProperty("FaAssetDepreciationExpenseAccounts")]
    public virtual AccChartAccount? DepreciationExpenseAccount { get; set; }

    [ForeignKey("DepreciationMethodId")]
    [InverseProperty("FaAssets")]
    public virtual CmnFaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaDepreciationRunLine> FaDepreciationRunLines { get; set; } = new List<FaDepreciationRunLine>();

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaDisposalDocLine> FaDisposalDocLines { get; set; } = new List<FaDisposalDocLine>();

    [ForeignKey("FaGroupId")]
    [InverseProperty("FaAssets")]
    public virtual CmnFaGroup FaGroup { get; set; } = null!;

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaMovementDocLine> FaMovementDocLines { get; set; } = new List<FaMovementDocLine>();

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaRevaluationDocLine> FaRevaluationDocLines { get; set; } = new List<FaRevaluationDocLine>();

    [ForeignKey("OkofId")]
    [InverseProperty("FaAssets")]
    public virtual CmnFaOkof? Okof { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaAssets")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("FaAssets")]
    public virtual SysUser? ResponsibleUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaAssets")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaAssets")]
    public virtual CmnFaAssetStatus Status { get; set; } = null!;
}
