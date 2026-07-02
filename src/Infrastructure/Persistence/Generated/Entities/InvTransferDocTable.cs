using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_transfer_doc_table")]
[Index("DestinationWarehouseId", Name = "idx_inv_transfer_doc_table_destination_warehouse_id")]
[Index("ProductTableId", Name = "idx_inv_transfer_doc_table_product_table_id")]
[Index("SourceWarehouseId", Name = "idx_inv_transfer_doc_table_source_warehouse_id")]
[Index("OwnerId", Name = "ix_inv_transfer_doc_table_owner_id")]
[Index("OwnerId", "ProductTableId", Name = "ux_inv_transfer_doc_table_owner_product_table", IsUnique = true)]
public partial class InvTransferDocTable
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

    [ForeignKey("DestinationWarehouseId")]
    [InverseProperty("InvTransferDocTableDestinationWarehouses")]
    public virtual InvWarehouse DestinationWarehouse { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [InverseProperty("InvTransferDocTables")]
    public virtual InvTransferLine Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("InvTransferDocTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("SourceWarehouseId")]
    [InverseProperty("InvTransferDocTableSourceWarehouses")]
    public virtual InvWarehouse SourceWarehouse { get; set; } = null!;
}
