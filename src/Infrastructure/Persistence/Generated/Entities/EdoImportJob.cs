using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_job")]
[Index("Status", "LeaseExpiresAt", Name = "idx_edo_import_job_status_lease")]
[Index("Id", "OrganizationId", Name = "ux_edo_import_job_id_organization", IsUnique = true)]
public partial class EdoImportJob
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("initiated_by_user_id")]
    public int InitiatedByUserId { get; set; }

    [Column("date_from")]
    public DateOnly DateFrom { get; set; }

    [Column("date_to")]
    public DateOnly DateTo { get; set; }

    [Column("status")]
    [StringLength(40)]
    public string Status { get; set; } = null!;

    [Column("scan_started_at", TypeName = "timestamp without time zone")]
    public DateTime? ScanStartedAt { get; set; }

    [Column("started_at", TypeName = "timestamp without time zone")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; set; }

    [Column("cancel_requested_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelRequestedAt { get; set; }

    [Column("lease_owner")]
    [StringLength(200)]
    public string? LeaseOwner { get; set; }

    [Column("lease_expires_at", TypeName = "timestamp without time zone")]
    public DateTime? LeaseExpiresAt { get; set; }

    [Column("heartbeat_at", TypeName = "timestamp without time zone")]
    public DateTime? HeartbeatAt { get; set; }

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; set; }

    [Column("safe_error_message")]
    [StringLength(1000)]
    public string? SafeErrorMessage { get; set; }

    [Column("discovered_count")]
    public int DiscoveredCount { get; set; }

    [Column("ready_count")]
    public int ReadyCount { get; set; }

    [Column("mapping_required_count")]
    public int MappingRequiredCount { get; set; }

    [Column("duplicate_count")]
    public int DuplicateCount { get; set; }

    [Column("imported_count")]
    public int ImportedCount { get; set; }

    [Column("failed_count")]
    public int FailedCount { get; set; }

    [Column("skipped_count")]
    public int SkippedCount { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [InverseProperty("EdoImportJob")]
    public virtual ICollection<EdoImportCandidate> EdoImportCandidates { get; set; } = new List<EdoImportCandidate>();

    [InverseProperty("Job")]
    public virtual ICollection<EdoImportJobProvider> EdoImportJobProviders { get; set; } = new List<EdoImportJobProvider>();

    [ForeignKey("InitiatedByUserId")]
    [InverseProperty("EdoImportJobs")]
    public virtual SysUser InitiatedByUser { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("EdoImportJob")]
    public virtual OrgOrganization Organization { get; set; } = null!;
}
