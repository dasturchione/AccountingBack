using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sale_doc_product")]
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

    [ForeignKey("CostAccountId")]
    [InverseProperty(nameof(ChartAccount.SaleDocProductCostAccounts))]
    public virtual ChartAccount? CostAccount { get; set; }

    [ForeignKey("IncomeAccountId")]
    [InverseProperty(nameof(ChartAccount.SaleDocProductIncomeAccounts))]
    public virtual ChartAccount? IncomeAccount { get; set; }

    [ForeignKey("InventoryAccountId")]
    [InverseProperty(nameof(ChartAccount.SaleDocProductInventoryAccounts))]
    public virtual ChartAccount? InventoryAccount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("SaleDocProducts")]
    public virtual SaleDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("SaleDocProducts")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<SaleDocTable> SaleDocTables { get; set; } = new List<SaleDocTable>();

    [InverseProperty(nameof(SaleDocProductBatch.SaleDocProduct))]
    public virtual ICollection<SaleDocProductBatch> SaleDocProductBatches { get; set; } = new List<SaleDocProductBatch>();

    [InverseProperty(nameof(SaleShipmentProduct.SaleDocProduct))]
    public virtual SaleShipmentProduct? SaleShipmentProduct { get; set; }

    [ForeignKey("UnitId")]
    [InverseProperty("SaleDocProducts")]
    public virtual Unit Unit { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("SaleDocProducts")]
    public virtual VatRate? VatRate { get; set; }
}
