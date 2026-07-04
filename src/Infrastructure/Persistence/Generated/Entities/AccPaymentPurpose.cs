using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_payment_purpose")]
[Index("Code", Name = "acc_payment_purpose_code_key", IsUnique = true)]
[Index("AliasId", Name = "idx_acc_payment_purpose_alias_id")]
[Index("OperationTypeId", Name = "idx_acc_payment_purpose_operation_type_id")]
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

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<AccPaymentPurposeTranslation> AccPaymentPurposeTranslations { get; set; } = new List<AccPaymentPurposeTranslation>();

    [ForeignKey("AliasId")]
    [InverseProperty("AccPaymentPurposes")]
    public virtual AccPostingAlias Alias { get; set; } = null!;

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<BankOperationLine> BankOperationLines { get; set; } = new List<BankOperationLine>();

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<BankOperation> BankOperations { get; set; } = new List<BankOperation>();

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<CashOperation> CashOperations { get; set; } = new List<CashOperation>();

    [InverseProperty("PaymentPurpose")]
    public virtual ICollection<CounterpartyAccountPaymentPurposeHint> CounterpartyAccountPaymentPurposeHints { get; set; } = new List<CounterpartyAccountPaymentPurposeHint>();

    [ForeignKey("OperationTypeId")]
    [InverseProperty("AccPaymentPurposes")]
    public virtual CmnOperationType OperationType { get; set; } = null!;
}
