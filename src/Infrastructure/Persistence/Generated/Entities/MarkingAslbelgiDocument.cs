using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("marking_aslbelgi_document")]
[Index("InternalDocumentType", "InternalDocumentId", Name = "idx_marking_aslbelgi_document_internal_document")]
[Index("OrganizationId", Name = "idx_marking_aslbelgi_document_organization_id")]
[Index("OrganizationId", "Status", Name = "idx_marking_aslbelgi_document_status")]
public partial class MarkingAslbelgiDocument
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("operation_type")]
    [StringLength(30)]
    public string OperationType { get; set; } = null!;

    [Column("internal_document_type")]
    [StringLength(50)]
    public string InternalDocumentType { get; set; } = null!;

    [Column("internal_document_id")]
    public long InternalDocumentId { get; set; }

    [Column("provider_document_id")]
    [StringLength(100)]
    public string? ProviderDocumentId { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("MarkingAslbelgiDocuments")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
