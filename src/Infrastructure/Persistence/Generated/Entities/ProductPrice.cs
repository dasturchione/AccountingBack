using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_price")]
public partial class ProductPrice
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("price_type_id")]
    public short PriceTypeId { get; set; }

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("price")]
    [Precision(24, 8)]
    public decimal Price { get; set; }

    [Column("start_date", TypeName = "timestamp without time zone")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "timestamp without time zone")]
    public DateTime? EndDate { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("ProductPrices")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("ProductPrices")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PriceTypeId")]
    [InverseProperty("ProductPrices")]
    public virtual ProductPriceType PriceType { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductPrices")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("ProductPrices")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("ProductPrices")]
    public virtual Unit Unit { get; set; } = null!;
}
