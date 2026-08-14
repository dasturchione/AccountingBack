using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[Table("rtl_sale_doc_payment")]
public partial class RetailSaleDocPayment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("payment_method_id")]
    public short PaymentMethodId { get; set; }

    [Column("bank_terminal_id")]
    public int? BankTerminalId { get; set; }

    [Column("debit_account_id")]
    public int DebitAccountId { get; set; }

    [Column("amount")]
    [Precision(24, 8)]
    public decimal Amount { get; set; }

    [Column("transaction_number")]
    [StringLength(100)]
    public string? TransactionNumber { get; set; }

    [ForeignKey(nameof(BankTerminalId))]
    [InverseProperty(nameof(BankTerminal.RetailSaleDocPayments))]
    public virtual BankTerminal? BankTerminal { get; set; }

    [ForeignKey(nameof(DebitAccountId))]
    [InverseProperty(nameof(ChartAccount.RetailSaleDocPayments))]
    public virtual ChartAccount DebitAccount { get; set; } = null!;

    [ForeignKey(nameof(OwnerId))]
    [InverseProperty(nameof(RetailSaleDoc.RetailSaleDocPayments))]
    public virtual RetailSaleDoc Owner { get; set; } = null!;

    [ForeignKey(nameof(PaymentMethodId))]
    [InverseProperty(nameof(PaymentMethod.RetailSaleDocPayments))]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
