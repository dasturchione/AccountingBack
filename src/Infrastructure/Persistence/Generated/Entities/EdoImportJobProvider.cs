using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_job_provider")]
[Index("Status", "NextRetryAt", Name = "idx_edo_import_job_provider_status_retry")]
[Index("JobId", "ProviderCode", Name = "ux_edo_import_job_provider_job_provider", IsUnique = true)]
public partial class EdoImportJobProvider
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_id")]
    public long JobId { get; set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = null!;

    [Column("status")]
    [StringLength(40)]
    public string Status { get; set; } = null!;

    [Column("current_page")]
    public int CurrentPage { get; set; }

    [Column("page_size")]
    public int PageSize { get; set; }

    [Column("provider_total")]
    public int? ProviderTotal { get; set; }

    [Column("scanned_count")]
    public int ScannedCount { get; set; }

    [Column("last_successful_page")]
    public int? LastSuccessfulPage { get; set; }

    [Column("last_attempt_at", TypeName = "timestamp without time zone")]
    public DateTime? LastAttemptAt { get; set; }

    [Column("next_retry_at", TypeName = "timestamp without time zone")]
    public DateTime? NextRetryAt { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("scan_completed_at", TypeName = "timestamp without time zone")]
    public DateTime? ScanCompletedAt { get; set; }

    [Column("is_waiting_auth")]
    public bool IsWaitingAuth { get; set; }

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [InverseProperty("EdoImportJobProvider")]
    public virtual ICollection<EdoImportCandidate> EdoImportCandidates { get; set; } = new List<EdoImportCandidate>();

    [ForeignKey("JobId")]
    [InverseProperty("EdoImportJobProviders")]
    public virtual EdoImportJob Job { get; set; } = null!;
}
