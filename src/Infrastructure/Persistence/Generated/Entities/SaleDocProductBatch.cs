using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_doc_product_batch")]
[Index("WarehouseProductBatchId", Name = "idx_sale_doc_product_batch_batch")]
[Index("SaleDocProductId", Name = "idx_sale_doc_product_batch_product")]
[Index("SaleDocProductId", "WarehouseProductBatchId", Name = "uq_sale_doc_product_batch", IsUnique = true)]
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
    [InverseProperty("SaleDocProductBatches")]
    public virtual SaleDocProduct SaleDocProduct { get; set; } = null!;

    [ForeignKey("WarehouseProductBatchId")]
    [InverseProperty("SaleDocProductBatches")]
    public virtual InvWarehouseProductBatch WarehouseProductBatch { get; set; } = null!;
}
