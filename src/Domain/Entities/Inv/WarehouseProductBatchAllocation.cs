using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_warehouse_product_batch_allocation")]
public partial class WarehouseProductBatchAllocation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("issue_movement_id")]
    public long IssueMovementId { get; set; }

    [Column("batch_id")]
    public long BatchId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("unit_cost")]
    [Precision(24, 8)]
    public decimal? UnitCost { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BatchId")]
    [InverseProperty(nameof(WarehouseProductBatch.WarehouseProductBatchAllocations))]
    public virtual WarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("IssueMovementId")]
    [InverseProperty(nameof(WarehouseProductMovement.WarehouseProductBatchAllocations))]
    public virtual WarehouseProductMovement IssueMovement { get; set; } = null!;
}
