using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("BatchId", "ProductTableId")]
[Table("inv_warehouse_product_batch_table")]
public partial class WarehouseProductBatchTable
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
    [InverseProperty(nameof(WarehouseProductBatch.WarehouseProductBatchTables))]
    public virtual WarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty(nameof(ProductTable.WarehouseProductBatchTables))]
    public virtual ProductTable ProductTable { get; set; } = null!;
}
