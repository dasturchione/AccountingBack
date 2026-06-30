using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("CounterpartyBankAccountId", "PaymentPurposeId")]
[Table("acc_counterparty_account_payment_purpose_hint")]
public partial class AccCounterpartyAccountPaymentPurposeHint
{
    [Key]
    [Column("counterparty_bank_account_id")]
    public int CounterpartyBankAccountId { get; set; }

    [Key]
    [Column("payment_purpose_id")]
    public short PaymentPurposeId { get; set; }

    [Column("usage_count")]
    public int UsageCount { get; set; }

    [Column("last_used_date", TypeName = "timestamp without time zone")]
    public DateTime LastUsedDate { get; set; }

    [ForeignKey("CounterpartyBankAccountId")]
    [InverseProperty("AccCounterpartyAccountPaymentPurposeHints")]
    public virtual CounterpartyBankAccount CounterpartyBankAccount { get; set; } = null!;

    [ForeignKey("PaymentPurposeId")]
    [InverseProperty("AccCounterpartyAccountPaymentPurposeHints")]
    public virtual AccPaymentPurpose PaymentPurpose { get; set; } = null!;
}
