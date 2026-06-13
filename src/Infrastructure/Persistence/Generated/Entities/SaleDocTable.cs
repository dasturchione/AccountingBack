using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_doc_table")]
[Index("OwnerId", Name = "idx_sale_doc_table_owner_id")]
[Index("ProductTableId", Name = "idx_sale_doc_table_product_id")]
[Index("VatRateId", Name = "idx_sale_doc_table_vat_rate_id")]
public partial class SaleDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("quantity")]
    [Precision(18, 3)]
    public decimal Quantity { get; set; }

    [Column("price")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

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
    [InverseProperty("SaleDocTables")]
    public virtual SaleDoc Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("SaleDocTables")]
    public virtual InvProductTable ProductTable { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("SaleDocTables")]
    public virtual CmnVatRate? VatRate { get; set; }
}
