using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_reg_entry")]
[Index("CreditAccountId", Name = "idx_acc_reg_entry_credit_account_id")]
[Index("CurrencyId", Name = "idx_acc_reg_entry_currency_id")]
[Index("DebitAccountId", Name = "idx_acc_reg_entry_debit_account_id")]
[Index("DocDate", Name = "idx_acc_reg_entry_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_acc_reg_entry_document")]
[Index("JournalNumber", Name = "idx_acc_reg_entry_journal_number")]
[Index("OperationTypeId", Name = "idx_acc_reg_entry_operation_type_id")]
[Index("OrganizationId", Name = "idx_acc_reg_entry_organization_id")]
public partial class AccountingRegisterEntry
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("debit_account_id")]
    public int? DebitAccountId { get; set; }

    [Column("credit_account_id")]
    public int? CreditAccountId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("operation_type_id")]
    public short? OperationTypeId { get; set; }

    [Column("debit_quantity")]
    [Precision(18, 3)]
    public decimal? DebitQuantity { get; set; }

    [Column("credit_quantity")]
    [Precision(18, 3)]
    public decimal? CreditQuantity { get; set; }

    [Column("content")]
    [StringLength(1000)]
    public string? Content { get; set; }

    [Column("journal_number")]
    [StringLength(100)]
    public string? JournalNumber { get; set; }

    [InverseProperty("Entry")]
    public virtual ICollection<RegisterEntrySubkonto> RegisterEntrySubkontos { get; set; } = new List<RegisterEntrySubkonto>();

    [ForeignKey("CreditAccountId")]
    [InverseProperty("RegisterEntryCreditAccounts")]
    public virtual ChartAccount? CreditAccount { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("AccountingRegisterEntries")]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("DebitAccountId")]
    [InverseProperty("RegisterEntryDebitAccounts")]
    public virtual ChartAccount? DebitAccount { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("AccountingRegisterEntries")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("AccountingRegisterEntries")]
    public virtual OperationType? OperationType { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccountingRegisterEntries")]
    public virtual Organization Organization { get; set; } = null!;
}
