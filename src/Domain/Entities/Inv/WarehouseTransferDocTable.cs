using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_transfer_doc_table")]
[Index("OwnerId", Name = "ix_inv_transfer_doc_table_owner_id")]
[Index("ProductTableId", Name = "idx_inv_transfer_doc_table_product_table_id")]
[Index("SourceWarehouseId", Name = "idx_inv_transfer_doc_table_source_warehouse_id")]
[Index("DestinationWarehouseId", Name = "idx_inv_transfer_doc_table_destination_warehouse_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_inv_transfer_doc_table_owner_product_table", IsUnique = true)]
public partial class WarehouseTransferDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("source_warehouse_id")]
    public int SourceWarehouseId { get; set; }

    [Column("destination_warehouse_id")]
    public int DestinationWarehouseId { get; set; }

    [Column("cost_price")]
    [Precision(24, 8)]
    public decimal CostPrice { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("WarehouseTransferDocTables")]
    public virtual WarehouseTransferLine Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    public virtual ProductTable ProductTable { get; set; } = null!;
}
