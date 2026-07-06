using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_doc_asset")]
[Index("DepartmentId", Name = "idx_fa_receipt_doc_asset_department_id")]
[Index("DepreciationMethodId", Name = "idx_fa_receipt_doc_asset_depreciation_method_id")]
[Index("FaAssetId", Name = "idx_fa_receipt_doc_asset_fa_asset_id")]
[Index("FaGroupId", Name = "idx_fa_receipt_doc_asset_fa_group_id")]
[Index("OkofId", Name = "idx_fa_receipt_doc_asset_okof_id")]
[Index("OwnerId", Name = "idx_fa_receipt_doc_asset_owner_id")]
[Index("ResponsibleUserId", Name = "idx_fa_receipt_doc_asset_responsible_user_id")]
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

    [Column("initial_cost")]
    [Precision(18, 2)]
    public decimal InitialCost { get; set; }

    [Column("salvage_value")]
    [Precision(18, 2)]
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

    [Column("planned_units_total")]
    [Precision(18, 3)]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("responsible_user_id")]
    public int? ResponsibleUserId { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual OrgDepartment? Department { get; set; }

    [ForeignKey("DepreciationMethodId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual CmnFaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [ForeignKey("FaAssetId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual FaAsset? FaAsset { get; set; }

    [ForeignKey("FaGroupId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual CmnFaGroup FaGroup { get; set; } = null!;

    [ForeignKey("OkofId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual CmnFaOkof? Okof { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual FaReceiptDocLine Owner { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("FaReceiptDocAssets")]
    public virtual SysUser? ResponsibleUser { get; set; }
}
