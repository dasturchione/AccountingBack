using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("WarehouseId", "ProductId")]
[Table("inv_warehouse_product")]
public partial class WarehouseProduct
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
    public decimal AvailableQuantity { get; set; }

    [Column("min_quantity")]
    [Precision(19, 6)]
    public decimal MinQuantity { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("WarehouseProducts")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("WarehouseProducts")]
    public virtual Unit Unit { get; set; } = null!;

    [ForeignKey("WarehouseId")]
    [InverseProperty("WarehouseProducts")]
    public virtual Warehouse Warehouse { get; set; } = null!;
}
