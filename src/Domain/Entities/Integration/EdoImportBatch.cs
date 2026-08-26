using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_batch")]
public sealed class EdoImportBatch
{
    private EdoImportBatch()
    {
    }

    public EdoImportBatch(
        int organizationId,
        string planHash,
        string? idempotencyKey,
        DateTime createdAt)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (string.IsNullOrWhiteSpace(planHash) || planHash.Length != 64)
            throw new ArgumentException("A SHA-256 plan hash is required.", nameof(planHash));

        OrganizationId = organizationId;
        ProviderCode = EdoImportBatchProviderCode.Edocs;
        PlanHash = planHash;
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
        Status = EdoImportBatchStatus.Planned;
        CreatedAt = createdAt;
    }

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; private set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; private set; } = EdoImportBatchProviderCode.Edocs;

    [Column("plan_hash")]
    [StringLength(64)]
    public string PlanHash { get; private set; } = null!;

    [Column("status")]
    [StringLength(30)]
    public string Status { get; private set; } = EdoImportBatchStatus.Planned;

    [Column("idempotency_key")]
    [StringLength(200)]
    public string? IdempotencyKey { get; private set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; private set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; private set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public ICollection<EdoImportBatchDocument> Documents { get; } = new List<EdoImportBatchDocument>();

    public void SetStatus(string status, DateTime now)
    {
        if (!EdoImportBatchStatus.IsDefined(status))
            throw new ArgumentException("Unknown batch status.", nameof(status));

        Status = status;
        UpdatedAt = now;
        CompletedAt = status is EdoImportBatchStatus.Completed or EdoImportBatchStatus.Failed
            ? now
            : null;
    }
}
