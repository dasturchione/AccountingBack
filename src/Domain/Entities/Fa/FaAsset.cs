using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_asset")]
[Index(nameof(DepartmentId), Name = "ix_fa_asset_department_id")]
[Index(nameof(FaGroupId), Name = "ix_fa_asset_fa_group_id")]
[Index(nameof(OkofId), Name = "ix_fa_asset_okof_id")]
[Index(nameof(OrganizationId), Name = "ix_fa_asset_organization_id")]
[Index(nameof(ResponsibleUserId), Name = "ix_fa_asset_responsible_user_id")]
[Index(nameof(StatusId), Name = "ix_fa_asset_status_id")]
[Index(nameof(OrganizationId), nameof(InventoryNumber), Name = "uq_fa_asset_org_inventory_number", IsUnique = true)]
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
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("fa_group_id")]
    public int FaGroupId { get; set; }

    [Column("okof_id")]
    public short? OkofId { get; set; }

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

    [InverseProperty(nameof(FaAssetAccounting.Asset))]
    public virtual FaAssetAccounting? FaAssetAccounting { get; set; }

    [InverseProperty(nameof(FaCommissioningDocLine.FaAsset))]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLines { get; set; } = new List<FaCommissioningDocLine>();

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

    [InverseProperty(nameof(FaReceiptDocAsset.FaAsset))]
    public virtual FaReceiptDocAsset? FaReceiptDocAsset { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("FaAssets")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("FaAssets")]
    public virtual FaAssetStatus Status { get; set; } = null!;

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaDepreciationRunLine> DepreciationRunLines { get; set; } = new List<FaDepreciationRunLine>();
}
