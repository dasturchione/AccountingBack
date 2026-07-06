using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_count_doc")]
[Index("DocDate", Name = "idx_inv_inventory_count_doc_doc_date")]
[Index("OrganizationId", Name = "idx_inv_inventory_count_doc_organization_id")]
[Index("StateId", Name = "idx_inv_inventory_count_doc_state_id")]
[Index("StatusId", Name = "idx_inv_inventory_count_doc_status_id")]
[Index("WarehouseId", Name = "idx_inv_inventory_count_doc_warehouse_id")]
[Index("OrganizationId", "DocNumber", Name = "ux_inv_inventory_count_doc_org_doc_number", IsUnique = true)]
public partial class InvInventoryCountDoc
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

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("count_completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CountCompletedAt { get; set; }

    [Column("count_completed_by_user_id")]
    public int? CountCompletedByUserId { get; set; }

    [Column("positive_adjustment_doc_id")]
    public long? PositiveAdjustmentDocId { get; set; }

    [Column("negative_adjustment_doc_id")]
    public long? NegativeAdjustmentDocId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime? PostedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<InvInventoryCountLine> InvInventoryCountLines { get; set; } = new List<InvInventoryCountLine>();

    [ForeignKey("NegativeAdjustmentDocId")]
    [InverseProperty("InvInventoryCountDocNegativeAdjustmentDocs")]
    public virtual InvInventoryAdjustmentDoc? NegativeAdjustmentDoc { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvInventoryCountDocs")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PositiveAdjustmentDocId")]
    [InverseProperty("InvInventoryCountDocPositiveAdjustmentDocs")]
    public virtual InvInventoryAdjustmentDoc? PositiveAdjustmentDoc { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("InvInventoryCountDocs")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("InvInventoryCountDocs")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvInventoryCountDocs")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
