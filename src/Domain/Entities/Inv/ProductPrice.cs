using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("inv_product_price")]
[Index("CurrencyId", Name = "idx_inv_product_price_currency_id")]
[Index("StartDate", "EndDate", Name = "idx_inv_product_price_dates")]
[Index("OrganizationId", Name = "idx_inv_product_price_organization_id")]
[Index("ProductId", Name = "idx_inv_product_price_product_id")]
[Index("StateId", Name = "idx_inv_product_price_state_id")]
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

    [Column("price")]
    [Precision(18, 2)]
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

    [ForeignKey("ProductId")]
    [InverseProperty("ProductPrices")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("ProductPrices")]
    public virtual State State { get; set; } = null!;
}
