using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("counterparty_reg_balance")]
[Index("CounterpartyId", Name = "idx_counterparty_reg_balance_counterparty_id")]
[Index("CurrencyId", Name = "idx_counterparty_reg_balance_currency_id")]
[Index("DocDate", Name = "idx_counterparty_reg_balance_doc_date")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_counterparty_reg_balance_document")]
[Index("OrganizationId", Name = "idx_counterparty_reg_balance_organization_id")]
[Index("PostingBatchId", Name = "idx_counterparty_reg_balance_posting_batch_id")]
[Index("ReversalEntryId", Name = "idx_counterparty_reg_balance_reversal_entry_id")]
public partial class CounterpartyRegBalance
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

    [Column("counterparty_id")]
    public int CounterpartyId { get; set; }

    [Column("operation_type_id")]
    public short OperationTypeId { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("doc_date", TypeName = "timestamp without time zone")]
    public DateTime DocDate { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("posting_batch_id")]
    public long? PostingBatchId { get; set; }

    [Column("source_line_id")]
    public long? SourceLineId { get; set; }

    [Column("reversal_entry_id")]
    public long? ReversalEntryId { get; set; }

    [ForeignKey("CounterpartyId")]
    [InverseProperty("CounterpartyRegBalances")]
    public virtual CounterpartyCard Counterparty { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty("CounterpartyRegBalances")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("CounterpartyRegBalances")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OperationTypeId")]
    [InverseProperty("CounterpartyRegBalances")]
    public virtual CmnOperationType OperationType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("CounterpartyRegBalances")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
