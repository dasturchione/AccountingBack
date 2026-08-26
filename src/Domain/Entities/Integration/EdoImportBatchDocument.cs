using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_batch_document")]
public sealed class EdoImportBatchDocument
{
    private EdoImportBatchDocument()
    {
    }

    public EdoImportBatchDocument(
        long batchId,
        int organizationId,
        string providerDocumentId,
        string direction,
        string status,
        DateTime createdAt,
        string documentType = "FACTURA",
        bool sentOverrideApplied = false)
    {
        if (batchId <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchId));
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (string.IsNullOrWhiteSpace(providerDocumentId))
            throw new ArgumentException("Provider document identity is required.", nameof(providerDocumentId));
        if (direction is not ("INBOX" or "OUTBOX"))
            throw new ArgumentException("Direction must be INBOX or OUTBOX.", nameof(direction));
        if (!EdoImportBatchDocumentStatus.IsDefined(status))
            throw new ArgumentException("Unknown batch document status.", nameof(status));

        BatchId = batchId;
        OrganizationId = organizationId;
        ProviderCode = EdoImportBatchProviderCode.Edocs;
        ProviderDocumentId = providerDocumentId.Trim();
        Direction = direction;
        DocumentType = EdoImportBatchDocumentStatus.NormalizeDocumentType(documentType);
        if (!EdoImportBatchDocumentStatus.IsSupportedDocumentType(DocumentType)
            && direction == "OUTBOX")
            throw new ArgumentException("Unsupported EDO document type.", nameof(documentType));
        Status = status;
        SentOverrideApplied = sentOverrideApplied;
        CreatedAt = createdAt;
    }

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("batch_id")]
    public long BatchId { get; private set; }

    [Column("organization_id")]
    public int OrganizationId { get; private set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; private set; } = EdoImportBatchProviderCode.Edocs;

    [Column("provider_document_id")]
    [StringLength(100)]
    public string ProviderDocumentId { get; private set; } = null!;

    [Column("edo_document_id")]
    public long? EdoDocumentId { get; private set; }

    [Column("direction")]
    [StringLength(10)]
    public string Direction { get; private set; } = null!;

    [Column("document_type")]
    [StringLength(50)]
    public string DocumentType { get; private set; } = "FACTURA";

    [Column("document_number")]
    [StringLength(100)]
    public string? DocumentNumber { get; private set; }

    [Column("document_date", TypeName = "date")]
    public DateOnly? DocumentDate { get; private set; }

    [Column("status")]
    [StringLength(40)]
    public string Status { get; private set; } = null!;

    [Column("purchase_document_id")]
    public long? PurchaseDocumentId { get; private set; }

    [Column("sale_document_id")]
    public long? SaleDocumentId { get; private set; }

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; private set; }

    [Column("has_marking")]
    public bool HasMarking { get; private set; }

    [Column("marking_count")]
    public int MarkingCount { get; private set; }

    [Column("marking_verification_state")]
    [StringLength(30)]
    public string? MarkingVerificationState { get; private set; }

    [Column("marking_source_type")]
    [StringLength(30)]
    public string? MarkingSourceType { get; private set; }

    [Column("sent_override_applied")]
    public bool SentOverrideApplied { get; private set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; private set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; private set; }

    public EdoImportBatch Batch { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;
    public EdoDocument? EdoDocument { get; private set; }
    public PurchaseDoc? PurchaseDocument { get; private set; }
    public SaleDoc? SaleDocument { get; private set; }

    public void SetSnapshot(
        long? edoDocumentId,
        string? documentNumber,
        DateOnly? documentDate,
        bool hasMarking,
        int markingCount,
        string? markingVerificationState,
        string? markingSourceType,
        bool sentOverrideApplied,
        DateTime now)
    {
        if (markingCount < 0)
            throw new ArgumentOutOfRangeException(nameof(markingCount));

        EdoDocumentId = edoDocumentId;
        DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim();
        DocumentDate = documentDate;
        HasMarking = hasMarking;
        MarkingCount = markingCount;
        MarkingVerificationState = string.IsNullOrWhiteSpace(markingVerificationState)
            ? null
            : markingVerificationState.Trim();
        MarkingSourceType = string.IsNullOrWhiteSpace(markingSourceType)
            ? null
            : markingSourceType.Trim();
        SentOverrideApplied = sentOverrideApplied;
        UpdatedAt = now;
    }

    public void SetResult(
        string status,
        long? purchaseDocumentId,
        long? saleDocumentId,
        string? safeErrorCode,
        DateTime now)
    {
        if (!EdoImportBatchDocumentStatus.IsDefined(status))
            throw new ArgumentException("Unknown batch document status.", nameof(status));
        if (purchaseDocumentId.HasValue && saleDocumentId.HasValue)
            throw new InvalidOperationException("A batch document cannot link both document directions.");
        if ((status is EdoImportBatchDocumentStatus.Imported or EdoImportBatchDocumentStatus.AlreadyImported) &&
            !purchaseDocumentId.HasValue && !saleDocumentId.HasValue)
            throw new InvalidOperationException("Imported batch document must have a local document link.");

        Status = status;
        PurchaseDocumentId = purchaseDocumentId;
        SaleDocumentId = saleDocumentId;
        SafeErrorCode = string.IsNullOrWhiteSpace(safeErrorCode) ? null : safeErrorCode.Trim();
        UpdatedAt = now;
    }
}
