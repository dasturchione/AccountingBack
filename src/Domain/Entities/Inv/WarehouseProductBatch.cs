using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_warehouse_product_batch")]
public partial class WarehouseProductBatch
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
    public virtual ICollection<WarehouseProductBatchAllocation> WarehouseProductBatchAllocations { get; set; } = new List<WarehouseProductBatchAllocation>();

    [InverseProperty(nameof(SaleDocProductBatch.WarehouseProductBatch))]
    public virtual ICollection<SaleDocProductBatch> SaleDocProductBatches { get; set; } = new List<SaleDocProductBatch>();

    [InverseProperty(nameof(WarehouseProductBatchTable.Batch))]
    public virtual ICollection<WarehouseProductBatchTable> WarehouseProductBatchTables { get; set; } = new List<WarehouseProductBatchTable>();

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.WarehouseProductBatches))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty(nameof(Product.WarehouseProductBatches))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("ReceiptMovementId")]
    [InverseProperty(nameof(WarehouseProductMovement.WarehouseProductBatch))]
    public virtual WarehouseProductMovement ReceiptMovement { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty(nameof(Warehouse.WarehouseProductBatches))]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
