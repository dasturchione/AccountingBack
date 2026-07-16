using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("BatchId", "ProductTableId")]
[Table("inv_warehouse_product_batch_table")]
[Index("ProductTableId", Name = "idx_inv_warehouse_product_batch_table_product_table_id")]
public partial class InvWarehouseProductBatchTable
{
    [Key]
    [Column("batch_id")]
    public long BatchId { get; set; }

    [Key]
    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BatchId")]
    [InverseProperty("InvWarehouseProductBatchTables")]
    public virtual InvWarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("InvWarehouseProductBatchTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;
}
