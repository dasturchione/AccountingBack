using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_doc_line")]
[Index("OwnerId", Name = "idx_fa_receipt_doc_line_owner_id")]
[Index("SourceProductId", Name = "idx_fa_receipt_doc_line_source_product_id")]
[Index("VatRateId", Name = "idx_fa_receipt_doc_line_vat_rate_id")]
[Index("CapitalInvestmentAccountId", Name = "ix_fa_receipt_line_capital_account")]
[Index("VatAccountId", Name = "ix_fa_receipt_line_vat_account")]
public partial class FaReceiptDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("source_product_id")]
    public int? SourceProductId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal Quantity { get; set; }

    [Column("price")]
    [Precision(24, 8)]
    public decimal Price { get; set; }

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

    [Column("capital_investment_account_id")]
    public int? CapitalInvestmentAccountId { get; set; }

    [Column("vat_account_id")]
    public int? VatAccountId { get; set; }

    [ForeignKey("CapitalInvestmentAccountId")]
    [InverseProperty("FaReceiptDocLineCapitalInvestmentAccounts")]
    public virtual AccChartAccount? CapitalInvestmentAccount { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("OwnerId")]
    [InverseProperty("FaReceiptDocLines")]
    public virtual FaReceiptDoc Owner { get; set; } = null!;

    [ForeignKey("SourceProductId")]
    [InverseProperty("FaReceiptDocLines")]
    public virtual InvProduct? SourceProduct { get; set; }

    [ForeignKey("VatAccountId")]
    [InverseProperty("FaReceiptDocLineVatAccounts")]
    public virtual AccChartAccount? VatAccount { get; set; }

    [ForeignKey("VatRateId")]
    [InverseProperty("FaReceiptDocLines")]
    public virtual CmnVatRate? VatRate { get; set; }
}
