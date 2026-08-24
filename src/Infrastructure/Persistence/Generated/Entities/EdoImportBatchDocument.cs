using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_batch_document")]
[Index("OrganizationId", "BatchId", "ProviderDocumentId", Name = "ux_edo_import_batch_document_organization_batch_provider_document", IsUnique = true)]
[Index("OrganizationId", "Status", Name = "idx_edo_import_batch_document_organization_status")]
public partial class EdoImportBatchDocument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("batch_id")]
    public long BatchId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = null!;

    [Column("provider_document_id")]
    [StringLength(100)]
    public string ProviderDocumentId { get; set; } = null!;

    [Column("edo_document_id")]
    public long? EdoDocumentId { get; set; }

    [Column("direction")]
    [StringLength(10)]
    public string Direction { get; set; } = null!;

    [Column("document_type")]
    [StringLength(50)]
    public string DocumentType { get; set; } = null!;

    [Column("document_number")]
    [StringLength(100)]
    public string? DocumentNumber { get; set; }

    [Column("document_date")]
    public DateOnly? DocumentDate { get; set; }

    [Column("status")]
    [StringLength(40)]
    public string Status { get; set; } = null!;

    [Column("purchase_document_id")]
    public long? PurchaseDocumentId { get; set; }

    [Column("sale_document_id")]
    public long? SaleDocumentId { get; set; }

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; set; }

    [Column("has_marking")]
    public bool HasMarking { get; set; }

    [Column("marking_count")]
    public int MarkingCount { get; set; }

    [Column("marking_verification_state")]
    [StringLength(30)]
    public string? MarkingVerificationState { get; set; }

    [Column("marking_source_type")]
    [StringLength(30)]
    public string? MarkingSourceType { get; set; }

    [Column("sent_override_applied")]
    public bool SentOverrideApplied { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BatchId")]
    [InverseProperty("EdoImportBatchDocuments")]
    public virtual EdoImportBatch Batch { get; set; } = null!;

    [ForeignKey("EdoDocumentId")]
    public virtual EdoDocument? EdoDocument { get; set; }

    [ForeignKey("OrganizationId")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual PurDoc? PurchaseDocument { get; set; }

    public virtual SaleDoc? SaleDocument { get; set; }
}
