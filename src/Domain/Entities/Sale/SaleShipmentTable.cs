using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sale_shipment_table")]
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
    [InverseProperty(nameof(ProductTable.SaleShipmentTables))]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey("ShipmentProductId")]
    [InverseProperty(nameof(SaleShipmentProduct.SaleShipmentTables))]
    public virtual SaleShipmentProduct ShipmentProduct { get; set; } = null!;
}
