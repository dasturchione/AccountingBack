using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_reg_entry")]
[Index("CreditAccountId", Name = "idx_acc_reg_entry_credit_account_id")]
[Index("CurrencyId", Name = "idx_acc_reg_entry_currency_id")]
[Index("DebitAccountId", Name = "idx_acc_reg_entry_debit_account_id")]
[Index("DocDate", Name = "idx_acc_reg_entry_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_acc_reg_entry_document")]
[Index("JournalNumber", Name = "idx_acc_reg_entry_journal_number")]
[Index("OperationTypeId", Name = "idx_acc_reg_entry_operation_type_id")]
[Index("OrganizationId", "CreditAccountId", "DocDate", "Id", Name = "idx_acc_reg_entry_org_credit_docdate_id")]
[Index("OrganizationId", "DebitAccountId", "DocDate", "Id", Name = "idx_acc_reg_entry_org_debit_docdate_id")]
[Index("OrganizationId", "DocDate", "CreditAccountId", Name = "idx_acc_reg_entry_org_docdate_credit_account")]
[Index("OrganizationId", "DocDate", "DebitAccountId", Name = "idx_acc_reg_entry_org_docdate_debit_account")]
[Index("OrganizationId", Name = "idx_acc_reg_entry_organization_id")]
[Index("PostingBatchId", Name = "idx_acc_reg_entry_posting_batch_id")]
[Index("ReversalEntryId", Name = "idx_acc_reg_entry_reversal_entry_id")]
public partial class AccRegEntry
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

    [Column("posting_batch_id")]
    public long? PostingBatchId { get; set; }

    [Column("source_line_id")]
    public long? SourceLineId { get; set; }

    [Column("reversal_entry_id")]
    public long? ReversalEntryId { get; set; }

    [InverseProperty("Entry")]
    public virtual ICollection<AccRegEntrySubkonto> AccRegEntrySubkontos { get; set; } = new List<AccRegEntrySubkonto>();

    [ForeignKey("CreditAccountId")]
    [InverseProperty("AccRegEntryCreditAccounts")]
    public virtual AccChartAccount? CreditAccount { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("AccRegEntries")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("DebitAccountId")]
    [InverseProperty("AccRegEntryDebitAccounts")]
    public virtual AccChartAccount? DebitAccount { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("AccRegEntries")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [InverseProperty("ReversalEntry")]
    public virtual ICollection<AccRegEntry> InverseReversalEntry { get; set; } = new List<AccRegEntry>();

    [ForeignKey("OperationTypeId")]
    [InverseProperty("AccRegEntries")]
    public virtual CmnOperationType? OperationType { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccRegEntries")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostingBatchId")]
    [InverseProperty("AccRegEntries")]
    public virtual AccPostingBatch? PostingBatch { get; set; }

    [ForeignKey("ReversalEntryId")]
    [InverseProperty("InverseReversalEntry")]
    public virtual AccRegEntry? ReversalEntry { get; set; }
}
