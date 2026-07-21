using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_shipment_table")]
[Index("ProductTableId", Name = "idx_sale_shipment_table_product_table_id")]
[Index("ShipmentProductId", Name = "idx_sale_shipment_table_shipment_product_id")]
[Index("ShipmentProductId", "ProductTableId", Name = "uq_sale_shipment_table_shipment_product", IsUnique = true)]
public partial class SaleShipmentTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("shipment_product_id")]
    public long ShipmentProductId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ProductTableId")]
    [InverseProperty("SaleShipmentTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("ShipmentProductId")]
    [InverseProperty("SaleShipmentTables")]
    public virtual SaleShipmentProduct ShipmentProduct { get; set; } = null!;
}
