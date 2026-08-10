using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("rtl_sale_doc_product")]
public partial class RetailSaleDocProduct
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("unit_price")]
    [Precision(24, 8)]
    public decimal UnitPrice { get; set; }

    [Column("cost_price")]
    [Precision(24, 8)]
    public decimal CostPrice { get; set; }

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

    [Column("unit_id")]
    public short UnitId { get; set; }

    [Column("inventory_account_id")]
    public int? InventoryAccountId { get; set; }

    [Column("income_account_id")]
    public int? IncomeAccountId { get; set; }

    [Column("cost_account_id")]
    public int? CostAccountId { get; set; }

    [ForeignKey(nameof(CostAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocProductCostAccounts))]
    public virtual ChartAccount? CostAccount { get; set; }

    [ForeignKey(nameof(IncomeAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocProductIncomeAccounts))]
    public virtual ChartAccount? IncomeAccount { get; set; }

    [ForeignKey(nameof(InventoryAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocProductInventoryAccounts))]
    public virtual ChartAccount? InventoryAccount { get; set; }

    [ForeignKey(nameof(OwnerId))]
    [InverseProperty(nameof(RetailSaleDoc.RetailSaleDocProducts))]
    public virtual RetailSaleDoc Owner { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    [InverseProperty(nameof(Product.RetailSaleDocProducts))]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty(nameof(RetailSaleDocTable.Owner))]
    public virtual ICollection<RetailSaleDocTable> RetailSaleDocTables { get; set; } = new List<RetailSaleDocTable>();

    [ForeignKey(nameof(UnitId))]
    [InverseProperty(nameof(Unit.RetailSaleDocProducts))]
    public virtual Unit Unit { get; set; } = null!;

    [ForeignKey(nameof(VatRateId))]
    [InverseProperty(nameof(VatRate.RetailSaleDocProducts))]
    public virtual VatRate? VatRate { get; set; }
}
