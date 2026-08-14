using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("rtl_sale_doc_table")]
public partial class RetailSaleDocTable
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_table_id")]
    public int ProductTableId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("vat_rate_id")]
    public short? VatRateId { get; set; }

    [Column("vat_amount")]
    [Precision(24, 8)]
    public decimal VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal TotalAmount { get; set; }

    [Column("cost_price")]
    [Precision(24, 8)]
    public decimal CostPrice { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    [InverseProperty(nameof(RetailSaleDocProduct.RetailSaleDocTables))]
    public virtual RetailSaleDocProduct Owner { get; set; } = null!;

    [ForeignKey(nameof(ProductTableId))]
    [InverseProperty(nameof(ProductTable.RetailSaleDocTables))]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey(nameof(VatRateId))]
    [InverseProperty(nameof(VatRate.RetailSaleDocTables))]
    public virtual VatRate? VatRate { get; set; }
}
