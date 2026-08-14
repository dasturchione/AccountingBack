using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_unit")]
public partial class Unit
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(State.Units))]
    public virtual State State { get; set; } = null!;

    [InverseProperty(nameof(Product.Unit))]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty(nameof(PurchaseDocProduct.Unit))]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty(nameof(ProductPrice.Unit))]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty(nameof(WarehouseProduct.Unit))]
    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

    [InverseProperty(nameof(SaleDocProduct.Unit))]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty(nameof(SaleShipmentProduct.Unit))]
    public virtual ICollection<SaleShipmentProduct> SaleShipmentProducts { get; set; } = new List<SaleShipmentProduct>();

    [InverseProperty(nameof(RetailSaleDocProduct.Unit))]
    public virtual ICollection<RetailSaleDocProduct> RetailSaleDocProducts { get; set; } = new List<RetailSaleDocProduct>();

    [InverseProperty(nameof(OpeningInventoryProduct.Unit))]
    public virtual ICollection<OpeningInventoryProduct> OpeningInventoryProducts { get; set; } = new List<OpeningInventoryProduct>();
}
