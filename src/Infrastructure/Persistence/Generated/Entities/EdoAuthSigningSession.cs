using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_auth_signing_session")]
[Index("ExpiresAt", Name = "idx_edo_auth_signing_session_expires_at")]
[Index("OrganizationId", "Provider", Name = "idx_edo_auth_signing_session_organization_provider")]
public partial class EdoAuthSigningSession
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

    [Column("challenge_id")]
    [StringLength(256)]
    public string ChallengeId { get; set; } = null!;

    [Column("provider_challenge_id")]
    [StringLength(256)]
    public string? ProviderChallengeId { get; set; }

    [Column("certificate_serial_number")]
    [StringLength(256)]
    public string? CertificateSerialNumber { get; set; }

    [Column("signing_mode")]
    [StringLength(40)]
    public string SigningMode { get; set; } = null!;

    [Column("expires_at", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("consumed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("EdoAuthSigningSessions")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
