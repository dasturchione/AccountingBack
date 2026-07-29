using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_opening_inventory_table")]
[Index("OwnerId", Name = "ix_inv_opening_inventory_table_owner_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_inv_opening_inventory_table_owner_id_product_table_id", IsUnique = true)]
public partial class InvOpeningInventoryTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("InvOpeningInventoryTables")]
    public virtual InvOpeningInventoryProduct Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("InvOpeningInventoryTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;
}
