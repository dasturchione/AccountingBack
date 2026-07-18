using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("ShipmentProductId", "BatchId")]
[Table("sale_shipment_product_batch")]
[Index("BatchId", Name = "idx_sale_shipment_product_batch_batch_id")]
public partial class SaleShipmentProductBatch
{
    [Key]
    [Column("shipment_product_id")]
    public long ShipmentProductId { get; set; }

    [Key]
    [Column("batch_id")]
    public long BatchId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [ForeignKey("BatchId")]
    [InverseProperty("SaleShipmentProductBatches")]
    public virtual InvWarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("ShipmentProductId")]
    [InverseProperty("SaleShipmentProductBatches")]
    public virtual SaleShipmentProduct ShipmentProduct { get; set; } = null!;
}
