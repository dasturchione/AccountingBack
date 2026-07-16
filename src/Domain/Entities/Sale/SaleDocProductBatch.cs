using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("sale_doc_product_batch")]
public partial class SaleDocProductBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("sale_doc_product_id")]
    public long SaleDocProductId { get; set; }

    [Column("warehouse_product_batch_id")]
    public long WarehouseProductBatchId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [ForeignKey("SaleDocProductId")]
    [InverseProperty(nameof(SaleDocProduct.SaleDocProductBatches))]
    public virtual SaleDocProduct SaleDocProduct { get; set; } = null!;

    [ForeignKey("WarehouseProductBatchId")]
    [InverseProperty(nameof(WarehouseProductBatch.SaleDocProductBatches))]
    public virtual WarehouseProductBatch WarehouseProductBatch { get; set; } = null!;
}
