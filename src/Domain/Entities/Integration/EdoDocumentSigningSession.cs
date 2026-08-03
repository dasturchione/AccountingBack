using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_document_signing_session")]
[Index(nameof(OrganizationId), nameof(Provider), nameof(DocumentId), Name = "idx_edo_document_signing_session_scope")]
public sealed class EdoDocumentSigningSession
{
    [Key]
    [Column("session_id")]
    [StringLength(64)]
    public string SessionId { get; set; } = null!;

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    [StringLength(20)]
    public string Provider { get; set; } = null!;

    [Column("document_id")]
    public long DocumentId { get; set; }

    [Column("signing_mode")]
    [StringLength(40)]
    public string SigningMode { get; set; } = null!;

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("consumed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(DocumentId))]
    public EdoDocument Document { get; set; } = null!;
}
