using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_batch")]
[Index("OrganizationId", "IdempotencyKey", Name = "ux_edo_import_batch_organization_idempotency", IsUnique = true)]
[Index("OrganizationId", "Status", Name = "idx_edo_import_batch_organization_status")]
[Index("Id", "OrganizationId", "ProviderCode", Name = "ux_edo_import_batch_id_organization_provider", IsUnique = true)]
public partial class EdoImportBatch
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = null!;

    [Column("plan_hash")]
    [StringLength(64)]
    public string PlanHash { get; set; } = null!;

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [Column("idempotency_key")]
    [StringLength(200)]
    public string? IdempotencyKey { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; set; }

    [InverseProperty("Batch")]
    public virtual ICollection<EdoImportBatchDocument> EdoImportBatchDocuments { get; set; } = new List<EdoImportBatchDocument>();

    [ForeignKey("OrganizationId")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
