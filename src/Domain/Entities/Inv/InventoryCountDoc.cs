using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_inventory_count_doc")]
[Index("OrganizationId", Name = "idx_inv_inventory_count_doc_organization_id")]
[Index("DocDate", Name = "idx_inv_inventory_count_doc_doc_date")]
[Index("WarehouseId", Name = "idx_inv_inventory_count_doc_warehouse_id")]
[Index("StatusId", Name = "idx_inv_inventory_count_doc_status_id")]
[Index("StateId", Name = "idx_inv_inventory_count_doc_state_id")]
public partial class InventoryCountDoc
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

    [ForeignKey("OrganizationId")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    public virtual Warehouse Warehouse { get; set; } = null!;

    [ForeignKey("StatusId")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("StateId")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("PositiveAdjustmentDocId")]
    public virtual InventoryAdjustmentDoc? PositiveAdjustmentDoc { get; set; }

    [ForeignKey("NegativeAdjustmentDocId")]
    public virtual InventoryAdjustmentDoc? NegativeAdjustmentDoc { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<InventoryCountLine> InventoryCountLines { get; set; } = new List<InventoryCountLine>();
}
