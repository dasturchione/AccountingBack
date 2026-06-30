using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("bank_operation")]
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

    [Column("counterparty_bank_account_id")]
    public int? CounterpartyBankAccountId { get; set; }

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
    public virtual BankAccount BankAccount { get; set; } = null!;

    [ForeignKey("CounterpartyId")]
    [InverseProperty("BankOperations")]
    public virtual CounterpartyCard? Counterparty { get; set; }

    [ForeignKey(nameof(CounterpartyBankAccountId))]
    [InverseProperty(nameof(CounterpartyBankAccount.BankOperations))]
    public virtual CounterpartyBankAccount? CounterpartyBankAccount { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("BankOperations")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("BankOperations")]
    public virtual OperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("BankOperations")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("BankOperations")]
    public virtual PaymentType? PaymentType { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("BankOperations")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("StatusId")]
    [InverseProperty("BankOperations")]
    public virtual DocumentStatus Status { get; set; } = null!;

    [InverseProperty(nameof(BankOperationLine.BankOperation))]
    public virtual ICollection<BankOperationLine> BankOperationLines { get; set; } = new List<BankOperationLine>();
}
