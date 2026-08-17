using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_candidate")]
public sealed class EdoImportCandidate
{
    private EdoImportCandidate()
    {
    }

    public EdoImportCandidate(
        long jobId,
        int organizationId,
        string providerCode,
        string providerDocumentId,
        DateTime createdDate)
    {
        if (jobId <= 0)
            throw new ArgumentOutOfRangeException(nameof(jobId));
        if (organizationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        if (string.IsNullOrWhiteSpace(providerCode))
            throw new ArgumentException("Provider code is required.", nameof(providerCode));
        if (string.IsNullOrWhiteSpace(providerDocumentId))
            throw new ArgumentException("Provider document ID is required.", nameof(providerDocumentId));

        JobId = jobId;
        OrganizationId = organizationId;
        ProviderCode = providerCode.Trim().ToUpperInvariant();
        ProviderDocumentId = providerDocumentId.Trim();
        Status = EdoImportCandidateStatus.Discovered;
        MappingStatus = EdoImportMappingStatus.Unresolved;
        DuplicateState = EdoImportDuplicateState.None;
        CreatedDate = createdDate;
    }

    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_id")]
    public long JobId { get; private set; }

    [Column("organization_id")]
    public int OrganizationId { get; private set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; private set; } = null!;

    [Column("provider_document_id")]
    [StringLength(100)]
    public string ProviderDocumentId { get; private set; } = null!;

    [Column("edo_document_id")]
    public long? EdoDocumentId { get; set; }

    [Column("direction")]
    [StringLength(10)]
    public string? Direction { get; set; }

    [Column("normalized_status")]
    [StringLength(40)]
    public string? NormalizedStatus { get; set; }

    [Column("document_type")]
    [StringLength(50)]
    public string? DocumentType { get; set; }

    [Column("document_number")]
    [StringLength(100)]
    public string? DocumentNumber { get; set; }

    [Column("document_date", TypeName = "date")]
    public DateOnly? DocumentDate { get; set; }

    [Column("document_date_time", TypeName = "timestamp without time zone")]
    public DateTime? DocumentDateTime { get; set; }

    [Column("seller_tin")]
    [StringLength(20)]
    public string? SellerTin { get; set; }

    [Column("buyer_tin")]
    [StringLength(20)]
    public string? BuyerTin { get; set; }

    [Column("seller_name")]
    [StringLength(500)]
    public string? SellerName { get; set; }

    [Column("provider_contract_number")]
    [StringLength(100)]
    public string? ProviderContractNumber { get; set; }

    [Column("provider_contract_date", TypeName = "date")]
    public DateOnly? ProviderContractDate { get; set; }

    [Column("net_amount")]
    [Precision(24, 8)]
    public decimal? NetAmount { get; set; }

    [Column("vat_amount")]
    [Precision(24, 8)]
    public decimal? VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal? TotalAmount { get; set; }

    [Column("shared_document_identity")]
    [StringLength(200)]
    public string? SharedDocumentIdentity { get; set; }

    [Column("header_fingerprint")]
    [StringLength(64)]
    public string? HeaderFingerprint { get; set; }

    [Column("content_fingerprint")]
    [StringLength(64)]
    public string? ContentFingerprint { get; set; }

    [Column("duplicate_state")]
    [StringLength(30)]
    public string DuplicateState { get; set; } = EdoImportDuplicateState.None;

    [Column("existing_purchase_id")]
    public long? ExistingPurchaseId { get; set; }

    [Column("selected_counterparty_id")]
    public int? SelectedCounterpartyId { get; set; }

    [Column("selected_contract_id")]
    public long? SelectedContractId { get; set; }

    [Column("selected_currency_id")]
    public short? SelectedCurrencyId { get; set; }

    [Column("selected_warehouse_id")]
    public int? SelectedWarehouseId { get; set; }

    [Column("mapping_status")]
    [StringLength(30)]
    public string MappingStatus { get; set; } = EdoImportMappingStatus.Unresolved;

    [Column("status")]
    [StringLength(40)]
    public string Status { get; private set; } = EdoImportCandidateStatus.Discovered;

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("imported_purchase_id")]
    public long? ImportedPurchaseId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; private set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; private set; }

    public EdoImportJob Job { get; private set; } = null!;

    public EdoImportJobProvider JobProvider { get; private set; } = null!;

    public Organization Organization { get; private set; } = null!;

    public EdoDocument? EdoDocument { get; set; }

    public PurchaseDoc? ExistingPurchase { get; set; }

    public PurchaseDoc? ImportedPurchase { get; set; }

    public CounterpartyCard? SelectedCounterparty { get; set; }

    public Contract? SelectedContract { get; set; }

    public Currency? SelectedCurrency { get; set; }

    public Warehouse? SelectedWarehouse { get; set; }

    public ICollection<EdoImportCandidateLine> Lines { get; } = new List<EdoImportCandidateLine>();

    public void TransitionTo(string requestedStatus, DateTime now)
    {
        if (!EdoImportCandidateStatus.IsDefined(requestedStatus)
            || !EdoImportCandidateStatus.CanTransition(Status, requestedStatus))
        {
            throw new EdoImportStateTransitionException(nameof(EdoImportCandidate), Status, requestedStatus);
        }

        Status = requestedStatus;
        UpdatedDate = now;
    }
}
