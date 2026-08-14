using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("fa_receipt_doc_line")]
[Index(nameof(CapitalInvestmentAccountId), Name = "ix_fa_receipt_doc_line_capital_investment_account_id")]
[Index(nameof(ReceiptDocId), Name = "ix_fa_receipt_doc_line_receipt_doc_id")]
[Index(nameof(VatAccountId), Name = "ix_fa_receipt_doc_line_vat_account_id")]
[Index(nameof(VatRateId), Name = "ix_fa_receipt_doc_line_vat_rate_id")]
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

    [ForeignKey(nameof(CapitalInvestmentAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocLineCapitalInvestmentAccounts))]
    public virtual ChartAccount? CapitalInvestmentAccount { get; set; }

    [InverseProperty(nameof(FaReceiptDocAsset.ReceiptDocLine))]
    public virtual ICollection<FaReceiptDocAsset> Assets { get; set; } = new List<FaReceiptDocAsset>();

    [ForeignKey(nameof(ReceiptDocId))]
    [InverseProperty(nameof(FaReceiptDoc.Lines))]
    public virtual FaReceiptDoc ReceiptDoc { get; set; } = null!;

    [ForeignKey(nameof(VatAccountId))]
    [InverseProperty(nameof(ChartAccount.FaReceiptDocLineVatAccounts))]
    public virtual ChartAccount? VatAccount { get; set; }

    [ForeignKey("VatRateId")]
    public virtual VatRate? VatRate { get; set; }
}
