using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("sale_shipment_product")]
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
    [InverseProperty(nameof(SaleShipmentDoc.SaleShipmentProducts))]
    public virtual SaleShipmentDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty(nameof(Product.SaleShipmentProducts))]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("SaleDocProductId")]
    [InverseProperty(nameof(SaleDocProduct.SaleShipmentProduct))]
    public virtual SaleDocProduct? SaleDocProduct { get; set; }

    [InverseProperty(nameof(SaleShipmentProductBatch.ShipmentProduct))]
    public virtual ICollection<SaleShipmentProductBatch> SaleShipmentProductBatches { get; set; } = new List<SaleShipmentProductBatch>();

    [InverseProperty(nameof(SaleShipmentTable.ShipmentProduct))]
    public virtual ICollection<SaleShipmentTable> SaleShipmentTables { get; set; } = new List<SaleShipmentTable>();

    [ForeignKey("UnitId")]
    [InverseProperty(nameof(Unit.SaleShipmentProducts))]
    public virtual Unit Unit { get; set; } = null!;
}
