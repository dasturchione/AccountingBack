using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public enum Provider
{
    Didox = 1,
    EDocs = 2,
    AslBelgi = 3
}

public enum CredentialKind
{
    PartnerToken = 1,
    CompanyToken = 2,
    TechnicalLogin = 3,
    TechnicalPassword = 4,
    ApiKey = 5,
    // Identity of an E-IMZO signing certificate (serial metadata only — never the private key/PFX).
    EImzoCertificate = 6,
    Other = 99
}

public enum CredentialStatus
{
    Active = 1,
    Rotating = 2,
    Revoked = 3,
    Expired = 4
}

[Table("int_provider_credential")]
[Index("OrganizationId", Name = "idx_int_provider_credential_organization_id")]
[Index("Provider", Name = "idx_int_provider_credential_provider")]
[Index("ExternalTin", Name = "idx_int_provider_credential_external_tin")]
[Index("Status", Name = "idx_int_provider_credential_status")]
public sealed class ProviderCredential
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

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

    [Column("credential_kind")]
    public CredentialKind CredentialKind { get; set; }

    [Column("encrypted_secret_reference")]
    [StringLength(1000)]
    public string EncryptedSecretReference { get; set; } = null!;

    [Column("key_version")]
    public int KeyVersion { get; set; }

    [Column("status")]
    public CredentialStatus Status { get; set; }

    [Column("valid_from_utc", TypeName = "timestamp without time zone")]
    public DateTime ValidFromUtc { get; set; }

    [Column("expires_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? ExpiresAtUtc { get; set; }

    [Column("revoked_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? RevokedAtUtc { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("created_at_utc", TypeName = "timestamp without time zone")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAtUtc { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedByUser { get; set; }
}
