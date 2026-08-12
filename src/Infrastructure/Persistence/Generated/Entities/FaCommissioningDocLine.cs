using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_commissioning_doc_line")]
[Index("AccumulatedDepreciationAccountId", Name = "ix_fa_commissioning_doc_line_accumulated_depreciation_account_i")]
[Index("CapitalInvestmentAccountId", Name = "ix_fa_commissioning_doc_line_capital_investment_account_id")]
[Index("CommissioningDocId", Name = "ix_fa_commissioning_doc_line_commissioning_doc_id")]
[Index("DepartmentId", Name = "ix_fa_commissioning_doc_line_department_id")]
[Index("DepreciationExpenseAccountId", Name = "ix_fa_commissioning_doc_line_depreciation_expense_account_id")]
[Index("DepreciationMethodId", Name = "ix_fa_commissioning_doc_line_depreciation_method_id")]
[Index("FaAssetId", Name = "ix_fa_commissioning_doc_line_fa_asset_id")]
[Index("ResponsibleUserId", Name = "ix_fa_commissioning_doc_line_responsible_user_id")]
[Index("CommissioningDocId", "FaAssetId", Name = "uq_fa_commissioning_doc_line_asset", IsUnique = true)]
public partial class FaCommissioningDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("commissioning_doc_id")]
    public long CommissioningDocId { get; set; }

    [Column("fa_asset_id")]
    public long FaAssetId { get; set; }

    [Column("capitalized_amount")]
    [Precision(24, 8)]
    public decimal CapitalizedAmount { get; set; }

    [Column("depr_start_date", TypeName = "timestamp without time zone")]
    public DateTime DeprStartDate { get; set; }

    [Column("salvage_value")]
    [Precision(24, 8)]
    public decimal SalvageValue { get; set; }

    [Column("useful_life_months")]
    public int UsefulLifeMonths { get; set; }

    [Column("depreciation_method_id")]
    public short DepreciationMethodId { get; set; }

    [Column("planned_units_total")]
    [Precision(24, 8)]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("capital_investment_account_id")]
    public int CapitalInvestmentAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int AccumulatedDepreciationAccountId { get; set; }

    [Column("depreciation_expense_account_id")]
    public int DepreciationExpenseAccountId { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("AccumulatedDepreciationAccountId")]
    [InverseProperty("FaCommissioningDocLineAccumulatedDepreciationAccounts")]
    public virtual AccChartAccount AccumulatedDepreciationAccount { get; set; } = null!;

    [ForeignKey("CapitalInvestmentAccountId")]
    [InverseProperty("FaCommissioningDocLineCapitalInvestmentAccounts")]
    public virtual AccChartAccount CapitalInvestmentAccount { get; set; } = null!;

    [ForeignKey("CommissioningDocId")]
    [InverseProperty("FaCommissioningDocLines")]
    public virtual FaCommissioningDoc CommissioningDoc { get; set; } = null!;

    [ForeignKey("DepartmentId")]
    [InverseProperty("FaCommissioningDocLines")]
    public virtual OrgDepartment? Department { get; set; }

    [ForeignKey("DepreciationExpenseAccountId")]
    [InverseProperty("FaCommissioningDocLineDepreciationExpenseAccounts")]
    public virtual AccChartAccount DepreciationExpenseAccount { get; set; } = null!;

    [ForeignKey("DepreciationMethodId")]
    [InverseProperty("FaCommissioningDocLines")]
    public virtual CmnFaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaCommissioningDocLines")]
    public virtual FaAsset FaAsset { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("FaCommissioningDocLines")]
    public virtual SysUser? ResponsibleUser { get; set; }
}
