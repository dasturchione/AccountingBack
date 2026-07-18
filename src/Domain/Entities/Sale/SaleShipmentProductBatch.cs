using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("ShipmentProductId", "BatchId")]
[Table("sale_shipment_product_batch")]
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
    [InverseProperty(nameof(WarehouseProductBatch.SaleShipmentProductBatches))]
    public virtual WarehouseProductBatch Batch { get; set; } = null!;

    [ForeignKey("ShipmentProductId")]
    [InverseProperty(nameof(SaleShipmentProduct.SaleShipmentProductBatches))]
    public virtual SaleShipmentProduct ShipmentProduct { get; set; } = null!;
}
