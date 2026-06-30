using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_payment_purpose")]
[Index("Code", Name = "acc_payment_purpose_code_key", IsUnique = true)]
public partial class AccPaymentPurpose
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("alias_id")]
    public short AliasId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("requires_counterparty")]
    public bool RequiresCounterparty { get; set; }

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<AccCounterpartyAccountPaymentPurposeHint> AccCounterpartyAccountPaymentPurposeHints { get; set; } = new List<AccCounterpartyAccountPaymentPurposeHint>();

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<AccPaymentPurposeTranslation> AccPaymentPurposeTranslations { get; set; } = new List<AccPaymentPurposeTranslation>();

    [ForeignKey("AliasId")]
    [InverseProperty("AccPaymentPurposes")]
    public virtual AccPostingAlias Alias { get; set; } = null!;

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<BankOperationLine> BankOperationLines { get; set; } = new List<BankOperationLine>();
}
