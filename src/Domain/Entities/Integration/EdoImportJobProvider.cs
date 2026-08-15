using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_job_provider")]
public sealed class EdoImportJobProvider
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
    public string Status { get; set; } = EdoImportProviderCheckpointStatus.Queued;

    [Column("current_page")]
    public int CurrentPage { get; set; } = 1;

    [Column("page_size")]
    public int PageSize { get; set; } = 20;

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

    public EdoImportJob Job { get; set; } = null!;

    public ICollection<EdoImportCandidate> Candidates { get; } = new List<EdoImportCandidate>();
}
