using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_document")]
[Index("OrganizationId", "Provider", "Direction", "Status", Name = "idx_edo_document_organization_status")]
public partial class EdoDocument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    [StringLength(20)]
    public string Provider { get; set; } = null!;

    [Column("direction")]
    [StringLength(10)]
    public string Direction { get; set; } = null!;

    [Column("internal_document_type")]
    [StringLength(50)]
    public string InternalDocumentType { get; set; } = null!;

    [Column("internal_document_id")]
    public long InternalDocumentId { get; set; }

    [Column("legacy_document_id")]
    public long? LegacyDocumentId { get; set; }

    [Column("provider_document_id")]
    [StringLength(100)]
    public string? ProviderDocumentId { get; set; }

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

    [Column("provider_status_code")]
    [StringLength(100)]
    public string? ProviderStatusCode { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("reject_reason")]
    [StringLength(2000)]
    public string? RejectReason { get; set; }

    [Column("operation_type")]
    [StringLength(100)]
    public string OperationType { get; set; } = null!;

    [Column("idempotency_key")]
    [StringLength(200)]
    public string? IdempotencyKey { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Document")]
    public virtual ICollection<EdoDocumentSigningSession> EdoDocumentSigningSessions { get; set; } = new List<EdoDocumentSigningSession>();

    [InverseProperty("EdoDocument")]
    public virtual ICollection<EdoImportCandidate> EdoImportCandidates { get; set; } = new List<EdoImportCandidate>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("EdoDocuments")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
