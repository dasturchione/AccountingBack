using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public enum ProviderSessionStatus
{
    Active = 1,
    Expired = 2,
    Revoked = 3
}

[Table("int_provider_session")]
[Index("OrganizationId", Name = "idx_int_provider_session_organization_id")]
[Index("ProviderCredentialId", Name = "idx_int_provider_session_provider_credential_id")]
[Index("Provider", Name = "idx_int_provider_session_provider")]
[Index("ExternalTin", Name = "idx_int_provider_session_external_tin")]
[Index("Status", Name = "idx_int_provider_session_status")]
public sealed class ProviderSession
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("provider_credential_id")]
    public long ProviderCredentialId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    public Provider Provider { get; set; }

    [Column("external_tin")]
    [StringLength(20)]
    public string ExternalTin { get; set; } = null!;

    [Column("entity_id")]
    [StringLength(200)]
    public string? EntityId { get; set; }

    /// <summary>Opaque secret-manager/envelope reference to the access token. Never the token itself.</summary>
    [Column("encrypted_access_token_reference")]
    [StringLength(1000)]
    public string EncryptedAccessTokenReference { get; set; } = null!;

    /// <summary>Opaque reference to the refresh token, when the provider issues one. Never the token itself.</summary>
    [Column("encrypted_refresh_token_reference")]
    [StringLength(1000)]
    public string? EncryptedRefreshTokenReference { get; set; }

    /// <summary>Non-reversible fingerprint (hash) of the access token, used only to match a 401 to this session.</summary>
    [Column("token_fingerprint")]
    [StringLength(128)]
    public string TokenFingerprint { get; set; } = null!;

    [Column("access_expires_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? AccessExpiresAtUtc { get; set; }

    [Column("refresh_expires_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? RefreshExpiresAtUtc { get; set; }

    [Column("credential_version")]
    public int CredentialVersion { get; set; }

    [Column("status")]
    public ProviderSessionStatus Status { get; set; }

    [Column("last_401_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? Last401AtUtc { get; set; }

    [Column("revoked_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? RevokedAtUtc { get; set; }

    [Column("created_at_utc", TypeName = "timestamp without time zone")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAtUtc { get; set; }

    [ForeignKey(nameof(ProviderCredentialId))]
    public ProviderCredential Credential { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
}
