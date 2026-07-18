using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_shipment_product")]
[Index("OwnerId", Name = "idx_sale_shipment_product_owner_id")]
[Index("ProductId", Name = "idx_sale_shipment_product_product_id")]
public partial class SaleShipmentProduct
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("sale_doc_product_id")]
    public long? SaleDocProductId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("SaleShipmentProducts")]
    public virtual SaleShipmentDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("SaleShipmentProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("SaleDocProductId")]
    [InverseProperty("SaleShipmentProduct")]
    public virtual SaleDocProduct? SaleDocProduct { get; set; }

    [InverseProperty("ShipmentProduct")]
    public virtual ICollection<SaleShipmentProductBatch> SaleShipmentProductBatches { get; set; } = new List<SaleShipmentProductBatch>();

    [InverseProperty("ShipmentProduct")]
    public virtual ICollection<SaleShipmentTable> SaleShipmentTables { get; set; } = new List<SaleShipmentTable>();

    [ForeignKey("UnitId")]
    [InverseProperty("SaleShipmentProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
