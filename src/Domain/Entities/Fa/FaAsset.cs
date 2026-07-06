using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_asset")]
[Index("OrganizationId", "InventoryNumber", Name = "uidx_fa_asset_org_inventory_number", IsUnique = true)]
[Index("StateId", Name = "idx_fa_asset_state_id")]
[Index("StatusId", Name = "idx_fa_asset_status_id")]
[Index("FaGroupId", Name = "idx_fa_asset_fa_group_id")]
[Index("OkofId", Name = "idx_fa_asset_okof_id")]
[Index("DepreciationMethodId", Name = "idx_fa_asset_depreciation_method_id")]
[Index("DepartmentId", Name = "idx_fa_asset_department_id")]
[Index("ResponsibleUserId", Name = "idx_fa_asset_responsible_user_id")]
[Index("SourceProductTableId", Name = "idx_fa_asset_source_product_table_id")]
[Index("CommissioningDate", Name = "idx_fa_asset_commissioning_date")]
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

    [Column("initial_cost", TypeName = "numeric(18,2)")]
    public decimal InitialCost { get; set; }

    [Column("salvage_value", TypeName = "numeric(18,2)")]
    public decimal SalvageValue { get; set; }

    [Column("commissioning_date", TypeName = "timestamp without time zone")]
    public DateTime? CommissioningDate { get; set; }

    [Column("depr_start_date", TypeName = "timestamp without time zone")]
    public DateTime? DeprStartDate { get; set; }

    [Column("planned_units_total", TypeName = "numeric(18,3)")]
    public decimal? PlannedUnitsTotal { get; set; }

    [Column("source_product_table_id")]
    public int? SourceProductTableId { get; set; }

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

    [ForeignKey("DepartmentId")]
    [InverseProperty("FaAssets")]
    public virtual Department? Department { get; set; }

    [ForeignKey("DepreciationMethodId")]
    [InverseProperty("FaAssets")]
    public virtual FaDepreciationMethod DepreciationMethod { get; set; } = null!;

    [ForeignKey("FaGroupId")]
    [InverseProperty("FaAssets")]
    public virtual FaGroup FaGroup { get; set; } = null!;

    [ForeignKey("OkofId")]
    [InverseProperty("FaAssets")]
    public virtual FaOkof? Okof { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("FaAssets")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ResponsibleUserId")]
    [InverseProperty("FaAssets")]
    public virtual User? ResponsibleUser { get; set; }

    [ForeignKey("SourceProductTableId")]
    [InverseProperty("FaAssets")]
    public virtual ProductTable? SourceProductTable { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaAssets")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaAssets")]
    public virtual FaAssetStatus Status { get; set; } = null!;
}
