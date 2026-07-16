using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_warehouse_product_batch")]
[Index("OrganizationId", "WarehouseId", "ProductId", Name = "idx_inv_warehouse_product_batch_warehouse_product")]
[Index("ReceiptMovementId", Name = "uq_inv_warehouse_product_batch_receipt_movement", IsUnique = true)]
public partial class InvWarehouseProductBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("receipt_movement_id")]
    public long ReceiptMovementId { get; set; }

    [Column("batch_number")]
    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [Column("initial_quantity")]
    [Precision(19, 6)]
    public decimal InitialQuantity { get; set; }

    [Column("remaining_quantity")]
    [Precision(19, 6)]
    public decimal RemainingQuantity { get; set; }

    [Column("unit_cost")]
    [Precision(24, 8)]
    public decimal? UnitCost { get; set; }

    [Column("received_date", TypeName = "timestamp without time zone")]
    public DateTime ReceivedDate { get; set; }

    [Column("expiry_date")]
    public DateOnly? ExpiryDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty("Batch")]
    public virtual ICollection<InvWarehouseProductBatchAllocation> InvWarehouseProductBatchAllocations { get; set; } = new List<InvWarehouseProductBatchAllocation>();

    [InverseProperty("Batch")]
    public virtual ICollection<InvWarehouseProductBatchTable> InvWarehouseProductBatchTables { get; set; } = new List<InvWarehouseProductBatchTable>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvWarehouseProductBatches")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvWarehouseProductBatches")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("ReceiptMovementId")]
    [InverseProperty("InvWarehouseProductBatch")]
    public virtual InvWarehouseProductMovement ReceiptMovement { get; set; } = null!;

    [InverseProperty("WarehouseProductBatch")]
    public virtual ICollection<SaleDocProductBatch> SaleDocProductBatches { get; set; } = new List<SaleDocProductBatch>();

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvWarehouseProductBatches")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
