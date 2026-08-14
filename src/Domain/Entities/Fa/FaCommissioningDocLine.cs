using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_commissioning_doc_line")]
[Index(nameof(AccumulatedDepreciationAccountId), Name = "ix_fa_commissioning_doc_line_accumulated_depreciation_account_i")]
[Index(nameof(CapitalInvestmentAccountId), Name = "ix_fa_commissioning_doc_line_capital_investment_account_id")]
[Index(nameof(CommissioningDocId), Name = "ix_fa_commissioning_doc_line_commissioning_doc_id")]
[Index(nameof(DepartmentId), Name = "ix_fa_commissioning_doc_line_department_id")]
[Index(nameof(DepreciationExpenseAccountId), Name = "ix_fa_commissioning_doc_line_depreciation_expense_account_id")]
[Index(nameof(DepreciationMethodId), Name = "ix_fa_commissioning_doc_line_depreciation_method_id")]
[Index(nameof(FaAssetId), Name = "ix_fa_commissioning_doc_line_fa_asset_id")]
[Index(nameof(ResponsibleUserId), Name = "ix_fa_commissioning_doc_line_responsible_user_id")]
[Index(nameof(CommissioningDocId), nameof(FaAssetId), Name = "uq_fa_commissioning_doc_line_asset", IsUnique = true)]
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

    [ForeignKey(nameof(AccumulatedDepreciationAccountId))]
    [InverseProperty(nameof(ChartAccount.FaCommissioningDocLineAccumulatedDepreciationAccounts))]
    public virtual ChartAccount AccumulatedDepreciationAccount { get; set; } = null!;

    [ForeignKey(nameof(CapitalInvestmentAccountId))]
    [InverseProperty(nameof(ChartAccount.FaCommissioningDocLineCapitalInvestmentAccounts))]
    public virtual ChartAccount CapitalInvestmentAccount { get; set; } = null!;

    [ForeignKey(nameof(CommissioningDocId))]
    [InverseProperty(nameof(FaCommissioningDoc.Lines))]
    public virtual FaCommissioningDoc CommissioningDoc { get; set; } = null!;

    [ForeignKey(nameof(DepartmentId))]
    [InverseProperty(nameof(Department.FaCommissioningDocLines))]
    public virtual Department? Department { get; set; }

    [ForeignKey(nameof(DepreciationExpenseAccountId))]
    [InverseProperty(nameof(ChartAccount.FaCommissioningDocLineDepreciationExpenseAccounts))]
    public virtual ChartAccount DepreciationExpenseAccount { get; set; } = null!;

    [ForeignKey(nameof(DepreciationMethodId))]
    [InverseProperty(nameof(FaDepreciationMethod.FaCommissioningDocLines))]
    public virtual FaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [ForeignKey(nameof(FaAssetId))]
    [InverseProperty(nameof(FaAsset.FaCommissioningDocLines))]
    public virtual FaAsset FaAsset { get; set; } = null!;

    [ForeignKey(nameof(ResponsibleUserId))]
    [InverseProperty(nameof(User.FaCommissioningDocLines))]
    public virtual User? ResponsibleUser { get; set; }
}
