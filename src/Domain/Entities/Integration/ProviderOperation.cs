using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public enum ProviderOperationState
{
    Pending = 1,
    Sent = 2,
    Confirmed = 3,
    Failed = 4,
    Unknown = 5,
    ReconcileRequired = 6
}

[Table("int_provider_operation")]
[Index("OrganizationId", Name = "idx_int_provider_operation_organization_id")]
[Index("Provider", Name = "idx_int_provider_operation_provider")]
[Index("State", Name = "idx_int_provider_operation_state")]
[Index("ExternalOperationId", Name = "idx_int_provider_operation_external_operation_id")]
public sealed class ProviderOperation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider")]
    public Provider Provider { get; set; }

    [Column("operation")]
    [StringLength(100)]
    public string Operation { get; set; } = null!;

    /// <summary>Caller-supplied idempotency key. Unique per organization/provider/operation.</summary>
    [Column("client_request_id")]
    [StringLength(200)]
    public string ClientRequestId { get; set; } = null!;

    /// <summary>Hash of the request payload — not the payload itself and never a token or secret.</summary>
    [Column("request_hash")]
    [StringLength(128)]
    public string RequestHash { get; set; } = null!;

    [Column("external_operation_id")]
    [StringLength(200)]
    public string? ExternalOperationId { get; set; }

    [Column("state")]
    public ProviderOperationState State { get; set; }

    [Column("reconcile_required")]
    public bool ReconcileRequired { get; set; }

    [Column("last_external_status")]
    [StringLength(100)]
    public string? LastExternalStatus { get; set; }

    [Column("last_error_code")]
    [StringLength(100)]
    public string? LastErrorCode { get; set; }

    [Column("created_at_utc", TypeName = "timestamp without time zone")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAtUtc { get; set; }

    [Column("completed_at_utc", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAtUtc { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
}
