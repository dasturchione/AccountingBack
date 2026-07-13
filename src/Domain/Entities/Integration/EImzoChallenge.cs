using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public enum EImzoChallengeState
{
    Pending = 1,
    Consumed = 2,
    Expired = 3
}

[Table("int_eimzo_challenge")]
[Index("ChallengeId", Name = "ux_int_eimzo_challenge_challenge_id", IsUnique = true)]
[Index("OrganizationId", "Provider", "ExternalTin", "EntityId", "State", Name = "idx_int_eimzo_challenge_scope_state")]
[Index("ExpiresAtUtc", "State", Name = "idx_int_eimzo_challenge_expiry_state")]
public sealed class EImzoChallenge
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("challenge_id")]
    [StringLength(128)]
    public string ChallengeId { get; set; } = null!;

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    public Provider Provider { get; set; }

    [Column("external_tin")]
    [StringLength(20)]
    public string ExternalTin { get; set; } = null!;

    /// <summary>Canonical certificate serial/entity identity. Never raw authId or PKCS#7.</summary>
    [Column("entity_id")]
    [StringLength(200)]
    public string EntityId { get; set; } = null!;

    /// <summary>SHA-256 hash of authId for E-DOCS challenges; never the authId itself.</summary>
    [Column("auth_id_hash")]
    [StringLength(128)]
    public string? AuthIdHash { get; set; }

    /// <summary>SHA-256 payload hash for the shared E-IMZO relay; never signed bytes.</summary>
    [Column("payload_hash")]
    [StringLength(128)]
    public string? PayloadHash { get; set; }

    [Column("sign_mode")]
    [StringLength(16)]
    public string? SignMode { get; set; }

    [Column("expires_at_utc", TypeName = "timestamp without time zone")]
    public DateTime ExpiresAtUtc { get; set; }

    [Column("consumed_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAtUtc { get; set; }

    [Column("state")]
    public EImzoChallengeState State { get; set; }

    [Column("created_at_utc", TypeName = "timestamp without time zone")]
    public DateTime CreatedAtUtc { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
}
