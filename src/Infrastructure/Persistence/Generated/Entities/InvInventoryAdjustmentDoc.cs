using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_adjustment_doc")]
[Index("AdjustmentType", Name = "idx_inv_inventory_adjustment_doc_adjustment_type")]
[Index("CancelledByUserId", Name = "idx_inv_inventory_adjustment_doc_cancelled_by_user_id")]
[Index("DocDate", Name = "idx_inv_inventory_adjustment_doc_doc_date")]
[Index("OrganizationId", Name = "idx_inv_inventory_adjustment_doc_organization_id")]
[Index("PostedByUserId", Name = "idx_inv_inventory_adjustment_doc_posted_by_user_id")]
[Index("StateId", Name = "idx_inv_inventory_adjustment_doc_state_id")]
[Index("StatusId", Name = "idx_inv_inventory_adjustment_doc_status_id")]
[Index("WarehouseId", Name = "idx_inv_inventory_adjustment_doc_warehouse_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_inv_inventory_adjustment_doc_org_doc_number", IsUnique = true)]
public partial class InvInventoryAdjustmentDoc
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("adjustment_type")]
    [StringLength(50)]
    public string AdjustmentType { get; set; } = null!;

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [ForeignKey("CancelledByUserId")]
    [InverseProperty("InvInventoryAdjustmentDocCancelledByUsers")]
    public virtual SysUser? CancelledByUser { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<InvInventoryAdjustmentLine> InvInventoryAdjustmentLines { get; set; } = new List<InvInventoryAdjustmentLine>();

    [InverseProperty("NegativeAdjustmentDoc")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocNegativeAdjustmentDocs { get; set; } = new List<InvInventoryCountDoc>();

    [InverseProperty("PositiveAdjustmentDoc")]
    public virtual ICollection<InvInventoryCountDoc> InvInventoryCountDocPositiveAdjustmentDocs { get; set; } = new List<InvInventoryCountDoc>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvInventoryAdjustmentDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("InvInventoryAdjustmentDocPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("InvInventoryAdjustmentDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("InvInventoryAdjustmentDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvInventoryAdjustmentDocs")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
