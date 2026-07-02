using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_inventory_adjustment_doc_table")]
[Index("OwnerId", Name = "ix_inv_inventory_adjustment_doc_table_owner_id")]
[Index("ProductTableId", Name = "idx_inv_inventory_adjustment_doc_table_product_table_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_inv_inventory_adjustment_doc_table_owner_product_table", IsUnique = true)]
public partial class InventoryAdjustmentDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int? ProductTableId { get; set; }

    [Column("cost_price")]
    [Precision(24, 8)]
    public decimal CostPrice { get; set; }

    [Column("original_status_id")]
    public short? OriginalStatusId { get; set; }

    [Column("original_state_id")]
    public short? OriginalStateId { get; set; }

    [Column("original_warehouse_id")]
    public int? OriginalWarehouseId { get; set; }

    [Column("was_created")]
    public bool WasCreated { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("InventoryAdjustmentDocTables")]
    public virtual InventoryAdjustmentLine Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    public virtual ProductTable? ProductTable { get; set; }
}
