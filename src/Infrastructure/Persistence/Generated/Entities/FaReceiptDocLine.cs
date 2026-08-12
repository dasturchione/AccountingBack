using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("fa_receipt_doc_line")]
[Index("CapitalInvestmentAccountId", Name = "ix_fa_receipt_doc_line_capital_investment_account_id")]
[Index("ReceiptDocId", Name = "ix_fa_receipt_doc_line_receipt_doc_id")]
[Index("VatAccountId", Name = "ix_fa_receipt_doc_line_vat_account_id")]
[Index("VatRateId", Name = "ix_fa_receipt_doc_line_vat_rate_id")]
public partial class FaReceiptDocLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("receipt_doc_id")]
    public long ReceiptDocId { get; set; }

    [Column("name")]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [Column("quantity")]
    public int Quantity { get; set; }

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

    [InverseProperty("ReceiptDocLine")]
    public virtual ICollection<FaReceiptDocAsset> FaReceiptDocAssets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey("ReceiptDocId")]
    [InverseProperty("FaReceiptDocLines")]
    public virtual FaReceiptDoc ReceiptDoc { get; set; } = null!;

    [ForeignKey("VatAccountId")]
    [InverseProperty("FaReceiptDocLineVatAccounts")]
    public virtual AccChartAccount? VatAccount { get; set; }

    [ForeignKey("VatRateId")]
    [InverseProperty("FaReceiptDocLines")]
    public virtual CmnVatRate? VatRate { get; set; }
}
