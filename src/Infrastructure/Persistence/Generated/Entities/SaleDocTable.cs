using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sale_doc_table")]
[Index("ProductTableId", Name = "idx_sale_doc_table_product_id")]
[Index("VatRateId", Name = "idx_sale_doc_table_vat_rate_id")]
[Index("OwnerId", Name = "ix_sale_doc_table_owner_id")]
public partial class SaleDocTable
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

    [ForeignKey("OwnerId")]
    [InverseProperty("SaleDocTables")]
    public virtual SaleDocProduct Owner { get; set; } = null!;

    [ForeignKey("ProductTableId")]
    [InverseProperty("SaleDocTables")]
    public virtual ProductTable ProductTable { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("SaleDocTables")]
    public virtual VatRate? VatRate { get; set; }
}
