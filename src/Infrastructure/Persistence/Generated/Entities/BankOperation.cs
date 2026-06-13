using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("bank_operation")]
[Index("BankAccountId", Name = "idx_bank_operation_bank_account_id")]
[Index("CounterpartyId", Name = "idx_bank_operation_counterparty_id")]
[Index("DocDate", Name = "idx_bank_operation_doc_date")]
[Index("OperationTypeId", Name = "idx_bank_operation_operation_type_id")]
[Index("OrganizationId", Name = "idx_bank_operation_organization_id")]
[Index("StateId", Name = "idx_bank_operation_state_id")]
[Index("StatusId", Name = "idx_bank_operation_status_id")]
public partial class BankOperation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("bank_account_id")]
    public int BankAccountId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("payment_type_id")]
    public short? PaymentTypeId { get; set; }

    [Column("counterparty_id")]
    public int? CounterpartyId { get; set; }

    [Column("doc_number")]
    [StringLength(100)]
    public string DocNumber { get; set; } = null!;

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("BankAccountId")]
    [InverseProperty("BankOperations")]
    public virtual OrgBankAccount BankAccount { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("BankOperations")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("BankOperations")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("BankOperations")]
    public virtual CmnOperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("BankOperations")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("BankOperations")]
    public virtual CmnPaymentType? PaymentType { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("BankOperations")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("BankOperations")]
    public virtual CmnDocumentStatus Status { get; set; } = null!;
}
