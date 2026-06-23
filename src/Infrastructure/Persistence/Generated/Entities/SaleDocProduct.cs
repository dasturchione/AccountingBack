using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_doc_product")]
[Index("OwnerId", Name = "ix_sale_doc_product_owner_id")]
[Index("ProductId", Name = "ix_sale_doc_product_product_id")]
public partial class SaleDocProduct
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("unit_price")]
    [Precision(18, 2)]
    public decimal UnitPrice { get; set; }

    [Column("cost_price")]
    [Precision(18, 2)]
    public decimal CostPrice { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("vat_rate_id")]
    public short? VatRateId { get; set; }

    [Column("vat_amount")]
    [Precision(18, 2)]
    public decimal VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("SaleDocProducts")]
    public virtual SaleDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("SaleDocProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [ForeignKey("VatRateId")]
    [InverseProperty("SaleDocProducts")]
    public virtual CmnVatRate? VatRate { get; set; }
}
