using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_warehouse_product_batch_allocation")]
[Index("BatchId", Name = "idx_inv_warehouse_product_batch_allocation_batch")]
[Index("IssueMovementId", Name = "idx_inv_warehouse_product_batch_allocation_movement")]
[Index("IssueMovementId", "BatchId", Name = "uq_inv_warehouse_product_batch_allocation", IsUnique = true)]
public partial class InvWarehouseProductBatchAllocation
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
    [InverseProperty("InvWarehouseProductBatchAllocations")]
    public virtual InvWarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("IssueMovementId")]
    [InverseProperty("InvWarehouseProductBatchAllocations")]
    public virtual InvWarehouseProductMovement IssueMovement { get; set; } = null!;
}
