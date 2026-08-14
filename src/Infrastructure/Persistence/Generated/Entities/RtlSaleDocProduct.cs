using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("rtl_sale_doc_product")]
[Index("CostAccountId", Name = "ix_rtl_sale_doc_product_cost_account_id")]
[Index("IncomeAccountId", Name = "ix_rtl_sale_doc_product_income_account_id")]
[Index("InventoryAccountId", Name = "ix_rtl_sale_doc_product_inventory_account_id")]
[Index("OwnerId", Name = "ix_rtl_sale_doc_product_owner_id")]
[Index("ProductId", Name = "ix_rtl_sale_doc_product_product_id")]
[Index("UnitId", Name = "ix_rtl_sale_doc_product_unit_id")]
[Index("VatRateId", Name = "ix_rtl_sale_doc_product_vat_rate_id")]
public partial class RtlSaleDocProduct
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
    [InverseProperty("RtlSaleDocProductCostAccounts")]
    public virtual AccChartAccount? CostAccount { get; set; }

    [ForeignKey("IncomeAccountId")]
    [InverseProperty("RtlSaleDocProductIncomeAccounts")]
    public virtual AccChartAccount? IncomeAccount { get; set; }

    [ForeignKey("InventoryAccountId")]
    [InverseProperty("RtlSaleDocProductInventoryAccounts")]
    public virtual AccChartAccount? InventoryAccount { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("RtlSaleDocProducts")]
    public virtual RtlSaleDoc Owner { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("RtlSaleDocProducts")]
    public virtual InvProduct Product { get; set; } = null!;

    [InverseProperty("Owner")]
    public virtual ICollection<RtlSaleDocTable> RtlSaleDocTables { get; set; } = new List<RtlSaleDocTable>();

    [ForeignKey("UnitId")]
    [InverseProperty("RtlSaleDocProducts")]
    public virtual CmnUnit Unit { get; set; } = null!;

    [ForeignKey("VatRateId")]
    [InverseProperty("RtlSaleDocProducts")]
    public virtual CmnVatRate? VatRate { get; set; }
}
