using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_inventory_count_line")]
[Index("OwnerId", Name = "ix_inv_inventory_count_line_owner_id")]
[Index("ProductId", Name = "idx_inv_inventory_count_line_product_id")]
[Index("UnitId", Name = "idx_inv_inventory_count_line_unit_id")]
[Index("OwnerId", "ProductId", "UnitId", Name = "ux_inv_inventory_count_line_owner_product_unit", IsUnique = true)]
public partial class InventoryCountLine
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

    [Column("counted_quantity")]
    [Precision(24, 8)]
    public decimal CountedQuantity { get; set; }

    [Column("default_cost_price")]
    [Precision(24, 8)]
    public decimal DefaultCostPrice { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("InventoryCountLines")]
    public virtual InventoryCountDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    public virtual Unit Unit { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<InventoryCountDocTable> InventoryCountDocTables { get; set; } = new List<InventoryCountDocTable>();
}
