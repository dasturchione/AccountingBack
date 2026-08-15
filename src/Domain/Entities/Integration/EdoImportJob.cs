using Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_job")]
public sealed class EdoImportJob
{
    private EdoImportJob()
    {
    }

    public EdoImportJob(
        int organizationId,
        int initiatedByUserId,
        DateOnly dateFrom,
        DateOnly dateTo,
        DateTime createdDate)
    {
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (initiatedByUserId <= 0)
            throw new ArgumentOutOfRangeException(nameof(initiatedByUserId));
        if (dateFrom > dateTo)
            throw new ArgumentException("DateFrom must not be later than DateTo.", nameof(dateFrom));

        OrganizationId = organizationId;
        InitiatedByUserId = initiatedByUserId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Status = EdoImportJobStatus.Queued;
        CreatedDate = createdDate;
    }

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; private set; }

    [Column("initiated_by_user_id")]
    public int InitiatedByUserId { get; private set; }

    [Column("date_from", TypeName = "date")]
    public DateOnly DateFrom { get; private set; }

    [Column("date_to", TypeName = "date")]
    public DateOnly DateTo { get; private set; }

    [Column("status")]
    [StringLength(40)]
    public string Status { get; private set; } = EdoImportJobStatus.Queued;

    [Column("scan_started_at", TypeName = "timestamp without time zone")]
    public DateTime? ScanStartedAt { get; private set; }

    [Column("started_at", TypeName = "timestamp without time zone")]
    public DateTime? StartedAt { get; private set; }

    [Column("completed_at", TypeName = "timestamp without time zone")]
    public DateTime? CompletedAt { get; private set; }

    [Column("cancel_requested_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelRequestedAt { get; private set; }

    [Column("lease_owner")]
    [StringLength(200)]
    public string? LeaseOwner { get; private set; }

    [Column("lease_expires_at", TypeName = "timestamp without time zone")]
    public DateTime? LeaseExpiresAt { get; private set; }

    [Column("heartbeat_at", TypeName = "timestamp without time zone")]
    public DateTime? HeartbeatAt { get; private set; }

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

    [Column("bulk_import_status")]
    [StringLength(30)]
    public string? BulkImportStatus { get; set; }

    [Column("bulk_batch_size")]
    public int? BulkBatchSize { get; set; }

    [Column("bulk_line_values_invalid_policy")]
    [StringLength(30)]
    public string? BulkLineValuesInvalidPolicy { get; set; }

    [Column("bulk_marking_already_used_policy")]
    [StringLength(80)]
    public string? BulkMarkingAlreadyUsedPolicy { get; set; }

    [Column("bulk_processed_count")]
    public int BulkProcessedCount { get; set; }

    [Column("bulk_created_draft_count")]
    public int BulkCreatedDraftCount { get; set; }

    [Column("bulk_reused_draft_count")]
    public int BulkReusedDraftCount { get; set; }

    [Column("bulk_duplicate_count")]
    public int BulkDuplicateCount { get; set; }

    [Column("bulk_skipped_count")]
    public int BulkSkippedCount { get; set; }

    [Column("bulk_failed_count")]
    public int BulkFailedCount { get; set; }

    [Column("bulk_last_safe_error_code")]
    [StringLength(100)]
    public string? BulkLastSafeErrorCode { get; set; }

    [Column("bulk_started_at", TypeName = "timestamp without time zone")]
    public DateTime? BulkStartedAt { get; set; }

    [Column("bulk_completed_at", TypeName = "timestamp without time zone")]
    public DateTime? BulkCompletedAt { get; set; }

    [Column("bulk_cancel_requested_at", TypeName = "timestamp without time zone")]
    public DateTime? BulkCancelRequestedAt { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; private set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public User InitiatedByUser { get; private set; } = null!;

    public ICollection<EdoImportJobProvider> Providers { get; } = new List<EdoImportJobProvider>();

    public ICollection<EdoImportCandidate> Candidates { get; } = new List<EdoImportCandidate>();

    public void TransitionTo(string requestedStatus, DateTime now)
    {
        if (!EdoImportJobStatus.IsDefined(requestedStatus)
            || !EdoImportJobStatus.CanTransition(Status, requestedStatus))
        {
            throw new EdoImportStateTransitionException(nameof(EdoImportJob), Status, requestedStatus);
        }

        Status = requestedStatus;
        UpdatedDate = now;

        if (requestedStatus == EdoImportJobStatus.Scanning)
        {
            StartedAt ??= now;
            ScanStartedAt ??= now;
        }
        else if (requestedStatus == EdoImportJobStatus.Importing)
        {
            StartedAt ??= now;
        }
        else if (requestedStatus == EdoImportJobStatus.CancelRequested)
        {
            CancelRequestedAt ??= now;
        }

        if (requestedStatus is EdoImportJobStatus.Completed
            or EdoImportJobStatus.Failed
            or EdoImportJobStatus.Cancelled)
        {
            CompletedAt ??= now;
            ClearLease(now);
        }
    }

    public void SetLease(string leaseOwner, DateTime leaseExpiresAt, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(leaseOwner))
            throw new ArgumentException("Lease owner is required.", nameof(leaseOwner));
        if (leaseExpiresAt <= now)
            throw new ArgumentOutOfRangeException(nameof(leaseExpiresAt));

        LeaseOwner = leaseOwner.Trim();
        LeaseExpiresAt = leaseExpiresAt;
        HeartbeatAt = now;
        UpdatedDate = now;
    }

    public void Heartbeat(DateTime leaseExpiresAt, DateTime now)
    {
        if (LeaseOwner is null)
            throw new InvalidOperationException("A lease must be acquired before heartbeat.");
        if (leaseExpiresAt <= now)
            throw new ArgumentOutOfRangeException(nameof(leaseExpiresAt));

        LeaseExpiresAt = leaseExpiresAt;
        HeartbeatAt = now;
        UpdatedDate = now;
    }

    public void ClearLease(DateTime now)
    {
        LeaseOwner = null;
        LeaseExpiresAt = null;
        HeartbeatAt = null;
        UpdatedDate = now;
    }
}
