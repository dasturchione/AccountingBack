using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("inv_product_price")]
[Index("CurrencyId", Name = "idx_inv_product_price_currency_id")]
[Index("StartDate", "EndDate", Name = "idx_inv_product_price_dates")]
[Index("OrganizationId", Name = "idx_inv_product_price_organization_id")]
[Index("PriceTypeId", Name = "idx_inv_product_price_price_type_id")]
[Index("ProductId", Name = "idx_inv_product_price_product_id")]
[Index("OrganizationId", "ProductId", "PriceTypeId", "StateId", "StartDate", "EndDate", Name = "idx_inv_product_price_product_type_dates")]
[Index("StateId", Name = "idx_inv_product_price_state_id")]
public partial class InvProductPrice
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
    [InverseProperty("InvProductPrices")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("InvProductPrices")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PriceTypeId")]
    [InverseProperty("InvProductPrices")]
    public virtual CmnProductPriceType PriceType { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("InvProductPrices")]
    public virtual InvProduct Product { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("InvProductPrices")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UnitId")]
    [InverseProperty("InvProductPrices")]
    public virtual CmnUnit Unit { get; set; } = null!;
}
