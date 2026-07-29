using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_opening_inventory_product")]
[Index("DebitAccountId", Name = "ix_inv_opening_inventory_product_account")]
[Index("OwnerId", Name = "ix_inv_opening_inventory_product_owner")]
[Index("ProductId", Name = "ix_inv_opening_inventory_product_product")]
[Index("OwnerId", "ProductId", Name = "ux_inv_opening_inventory_product_owner_product", IsUnique = true)]
public partial class InvOpeningInventoryProduct
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("unit_price")]
    [Precision(24, 8)]
    public decimal UnitPrice { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("debit_account_id")]
    public int DebitAccountId { get; set; }

    [ForeignKey("DebitAccountId")]
    [InverseProperty("InvOpeningInventoryProducts")]
    public virtual AccChartAccount DebitAccount { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<InvOpeningInventoryTable> InvOpeningInventoryTables { get; set; } = new List<InvOpeningInventoryTable>();

    [ForeignKey("OwnerId")]
    [InverseProperty("InvOpeningInventoryProducts")]
    public virtual InvOpeningInventory Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvOpeningInventoryProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvOpeningInventoryProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
