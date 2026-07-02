using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_adjustment_line")]
[Index("OwnerId", Name = "ix_inv_inventory_adjustment_line_owner_id")]
[Index("ProductId", Name = "idx_inv_inventory_adjustment_line_product_id")]
[Index("UnitId", Name = "idx_inv_inventory_adjustment_line_unit_id")]
public partial class InventoryAdjustmentLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("quantity")]
    [Precision(24, 8)]
    public decimal Quantity { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("InventoryAdjustmentLines")]
    public virtual InventoryAdjustmentDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    public virtual Unit Unit { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<InventoryAdjustmentDocTable> InventoryAdjustmentDocTables { get; set; } = new List<InventoryAdjustmentDocTable>();
}
