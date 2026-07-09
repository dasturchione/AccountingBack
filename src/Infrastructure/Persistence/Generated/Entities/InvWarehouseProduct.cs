using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("WarehouseId", "ProductId")]
[Table("inv_warehouse_product")]
[Index("ProductId", Name = "idx_inv_warehouse_product_product_id")]
[Index("UnitId", Name = "idx_inv_warehouse_product_unit_id")]
public partial class InvWarehouseProduct
{
    [Key]
    [Column("warehouse_id")]
    public int WarehouseId { get; set; }

    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("reserved_quantity")]
    [Precision(19, 6)]
    public decimal ReservedQuantity { get; set; }

    [Column("blocked_quantity")]
    [Precision(19, 6)]
    public decimal BlockedQuantity { get; set; }

    [Column("available_quantity")]
    [Precision(19, 6)]
    public decimal? AvailableQuantity { get; set; }

    [Column("min_quantity")]
    [Precision(19, 6)]
    public decimal MinQuantity { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("InvWarehouseProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvWarehouseProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("InvWarehouseProducts")]
    public virtual InvWarehouse Warehouse { get; set; } = null!;
}
