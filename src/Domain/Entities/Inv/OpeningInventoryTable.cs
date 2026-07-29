using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_opening_inventory_table")]
public partial class OpeningInventoryTable
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
    [InverseProperty(nameof(OpeningInventoryProduct.OpeningInventoryTables))]
    public virtual OpeningInventoryProduct Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty(nameof(ProductTable.OpeningInventoryTables))]
    public virtual ProductTable ProductTable { get; set; } = null!;
}
