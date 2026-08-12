using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_asset")]
[Index("DepartmentId", Name = "ix_fa_asset_department_id")]
[Index("FaGroupId", Name = "ix_fa_asset_fa_group_id")]
[Index("OkofId", Name = "ix_fa_asset_okof_id")]
[Index("OrganizationId", Name = "ix_fa_asset_organization_id")]
[Index("ResponsibleUserId", Name = "ix_fa_asset_responsible_user_id")]
[Index("StatusId", Name = "ix_fa_asset_status_id")]
[Index("OrganizationId", "InventoryNumber", Name = "uq_fa_asset_org_inventory_number", IsUnique = true)]
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
    public virtual OrgDepartment? Department { get; set; }

    [InverseProperty("Asset")]
    public virtual FaAssetAccounting? FaAssetAccounting { get; set; }

    [InverseProperty("FaAsset")]
    public virtual ICollection<FaCommissioningDocLine> FaCommissioningDocLines { get; set; } = new List<FaCommissioningDocLine>();

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
    public virtual FaReceiptDocAsset? FaReceiptDocAsset { get; set; }

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
