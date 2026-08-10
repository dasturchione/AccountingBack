using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("rtl_sale_doc_payment")]
[Index("BankTerminalId", Name = "ix_rtl_sale_doc_payment_bank_terminal_id")]
[Index("DebitAccountId", Name = "ix_rtl_sale_doc_payment_debit_account_id")]
[Index("OwnerId", Name = "ix_rtl_sale_doc_payment_owner_id")]
[Index("PaymentMethodId", Name = "ix_rtl_sale_doc_payment_payment_method_id")]
public partial class RtlSaleDocPayment
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

    [ForeignKey("BankTerminalId")]
    [InverseProperty("RtlSaleDocPayments")]
    public virtual BankTerminal? BankTerminal { get; set; }

    [ForeignKey("DebitAccountId")]
    [InverseProperty("RtlSaleDocPayments")]
    public virtual AccChartAccount DebitAccount { get; set; } = null!;

    [ForeignKey("OwnerId")]
    [InverseProperty("RtlSaleDocPayments")]
    public virtual RtlSaleDoc Owner { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("RtlSaleDocPayments")]
    public virtual RtlPaymentMethod PaymentMethod { get; set; } = null!;
}
