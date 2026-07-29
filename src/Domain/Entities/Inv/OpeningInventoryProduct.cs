using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("inv_opening_inventory_product")]
public partial class OpeningInventoryProduct
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
    [InverseProperty(nameof(ChartAccount.OpeningInventoryProducts))]
    public virtual ChartAccount DebitAccount { get; set; } = null!;

    [InverseProperty(nameof(OpeningInventoryTable.Owner))]
    public virtual ICollection<OpeningInventoryTable> OpeningInventoryTables { get; set; } = new List<OpeningInventoryTable>();

    [ForeignKey("OwnerId")]
    [InverseProperty(nameof(OpeningInventory.OpeningInventoryProducts))]
    public virtual OpeningInventory Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty(nameof(Product.OpeningInventoryProducts))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty(nameof(Unit.OpeningInventoryProducts))]
    public virtual Unit Unit { get; set; } = null!;
}
