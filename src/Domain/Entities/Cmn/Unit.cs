using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_unit")]
[Index("Code", Name = "idx_cmn_unit_code", IsUnique = true)]
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

    [InverseProperty("Unit")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    [InverseProperty("Unit")]
    public virtual ICollection<PurchaseDocProduct> PurchaseDocProducts { get; set; } = new List<PurchaseDocProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    [InverseProperty("Unit")]
    public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

    [InverseProperty("Unit")]
    public virtual ICollection<SaleDocProduct> SaleDocProducts { get; set; } = new List<SaleDocProduct>();

    [InverseProperty(nameof(SaleShipmentProduct.Unit))]
    public virtual ICollection<SaleShipmentProduct> SaleShipmentProducts { get; set; } = new List<SaleShipmentProduct>();

    [InverseProperty(nameof(OpeningInventoryProduct.Unit))]
    public virtual ICollection<OpeningInventoryProduct> OpeningInventoryProducts { get; set; } = new List<OpeningInventoryProduct>();

    [ForeignKey("StateId")]
    [InverseProperty("Units")]
    public virtual State State { get; set; } = null!;
}
