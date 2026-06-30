using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("acc_posting_batch")]
[Index("DocumentTypeId", "DocumentId", Name = "idx_acc_posting_batch_document")]
[Index("OrganizationId", Name = "idx_acc_posting_batch_organization_id")]
[Index("Status", Name = "idx_acc_posting_batch_status")]
public partial class AccPostingBatch
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

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("posted_at", TypeName = "timestamp without time zone")]
    public DateTime PostedAt { get; set; }

    [Column("reversed_by_user_id")]
    public int? ReversedByUserId { get; set; }

    [Column("reversed_at", TypeName = "timestamp without time zone")]
    public DateTime? ReversedAt { get; set; }

    [Column("comment")]
    [StringLength(1000)]
    public string? Comment { get; set; }

    [InverseProperty("PostingBatch")]
    public virtual ICollection<AccRegEntry> AccRegEntries { get; set; } = new List<AccRegEntry>();

    [InverseProperty("PostingBatch")]
    public virtual ICollection<CounterpartyRegBalance> CounterpartyRegBalances { get; set; } = new List<CounterpartyRegBalance>();

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("AccPostingBatches")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [InverseProperty("PostingBatch")]
    public virtual ICollection<InvRegBalance> InvRegBalances { get; set; } = new List<InvRegBalance>();

    [InverseProperty("PostingBatch")]
    public virtual ICollection<MoneyRegBalance> MoneyRegBalances { get; set; } = new List<MoneyRegBalance>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("AccPostingBatches")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("PostedByUserId")]
    [InverseProperty("AccPostingBatchPostedByUsers")]
    public virtual SysUser? PostedByUser { get; set; }

    [ForeignKey("ReversedByUserId")]
    [InverseProperty("AccPostingBatchReversedByUsers")]
    public virtual SysUser? ReversedByUser { get; set; }
}
