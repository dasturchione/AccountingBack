using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_adjustment_line")]
[Index("ProductId", Name = "idx_inv_inventory_adjustment_line_product_id")]
[Index("UnitId", Name = "idx_inv_inventory_adjustment_line_unit_id")]
[Index("OwnerId", Name = "ix_inv_inventory_adjustment_line_owner_id")]
public partial class InvInventoryAdjustmentLine
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

    [InverseProperty("Owner")]
    public virtual ICollection<InvInventoryAdjustmentDocTable> InvInventoryAdjustmentDocTables { get; set; } = new List<InvInventoryAdjustmentDocTable>();

    [ForeignKey("OwnerId")]
    [InverseProperty("InvInventoryAdjustmentLines")]
    public virtual InvInventoryAdjustmentDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvInventoryAdjustmentLines")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvInventoryAdjustmentLines")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
