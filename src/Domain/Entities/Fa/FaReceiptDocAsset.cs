using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_doc_asset")]
public partial class FaReceiptDocAsset
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("fa_asset_id")]
    public long? FaAssetId { get; set; }

    [Column("inventory_number")]
    [StringLength(100)]
    public string InventoryNumber { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("initial_cost", TypeName = "numeric(18,2)")]
    public decimal InitialCost { get; set; }

    [Column("salvage_value", TypeName = "numeric(18,2)")]
    public decimal SalvageValue { get; set; }

    [Column("useful_life_months")]
    public int UsefulLifeMonths { get; set; }

    [Column("depreciation_method_id")]
    public short DepreciationMethodId { get; set; }

    [Column("fa_group_id")]
    public int FaGroupId { get; set; }

    [Column("okof_id")]
    public short? OkofId { get; set; }

    [Column("commissioning_date", TypeName = "timestamp without time zone")]
    public DateTime? CommissioningDate { get; set; }

    [Column("depr_start_date", TypeName = "timestamp without time zone")]
    public DateTime? DeprStartDate { get; set; }

    [Column("planned_units_total", TypeName = "numeric(18,3)")]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [Column("asset_account_id")]
    public int? AssetAccountId { get; set; }

    [Column("accumulated_depreciation_account_id")]
    public int? AccumulatedDepreciationAccountId { get; set; }

    [Column("depreciation_expense_account_id")]
    public int? DepreciationExpenseAccountId { get; set; }

    [ForeignKey(nameof(AssetAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocAssetAssetAccounts))]
    public virtual ChartAccount? AssetAccount { get; set; }

    [ForeignKey(nameof(AccumulatedDepreciationAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocAssetAccumulatedDepreciationAccounts))]
    public virtual ChartAccount? AccumulatedDepreciationAccount { get; set; }

    [ForeignKey(nameof(DepreciationExpenseAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocAssetDepreciationExpenseAccounts))]
    public virtual ChartAccount? DepreciationExpenseAccount { get; set; }

    [ForeignKey("DepartmentId")]
    public virtual Department? Department { get; set; }

    [ForeignKey("DepreciationMethodId")]
    public virtual FaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    public virtual FaAsset? FaAsset { get; set; }

    [ForeignKey("FaGroupId")]
    public virtual FaGroup FaGroup { get; set; } = null!;

    [ForeignKey("OkofId")]
    public virtual FaOkof? Okof { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("Assets")]
    public virtual FaReceiptDocLine Owner { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    public virtual User? ResponsibleUser { get; set; }
}
