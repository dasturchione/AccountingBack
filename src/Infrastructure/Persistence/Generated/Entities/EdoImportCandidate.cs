using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_candidate")]
[Index("JobId", "Status", "MappingStatus", Name = "idx_edo_import_candidate_job_status_mapping")]
[Index("Status", "AttemptCount", Name = "idx_edo_import_candidate_status_attempt")]
[Index("JobId", "ProviderCode", "ProviderDocumentId", Name = "ux_edo_import_candidate_job_provider_document", IsUnique = true)]
public partial class EdoImportCandidate
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("job_id")]
    public long JobId { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("provider_code")]
    [StringLength(20)]
    public string ProviderCode { get; set; } = null!;

    [Column("provider_document_id")]
    [StringLength(100)]
    public string ProviderDocumentId { get; set; } = null!;

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

    [Column("document_date")]
    public DateOnly? DocumentDate { get; set; }

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

    [Column("provider_contract_date")]
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
    public string DuplicateState { get; set; } = null!;

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
    public string MappingStatus { get; set; } = null!;

    [Column("status")]
    [StringLength(40)]
    public string Status { get; set; } = null!;

    [Column("safe_error_code")]
    [StringLength(100)]
    public string? SafeErrorCode { get; set; }

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("imported_purchase_id")]
    public long? ImportedPurchaseId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("EdoDocumentId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual EdoDocument? EdoDocument { get; set; }

    [InverseProperty("Candidate")]
    public virtual ICollection<EdoImportCandidateLine> EdoImportCandidateLines { get; set; } = new List<EdoImportCandidateLine>();

    [ForeignKey("JobId, OrganizationId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual EdoImportJob EdoImportJob { get; set; } = null!;

    [ForeignKey("JobId, ProviderCode")]
    [InverseProperty("EdoImportCandidates")]
    public virtual EdoImportJobProvider EdoImportJobProvider { get; set; } = null!;

    [ForeignKey("ExistingPurchaseId")]
    [InverseProperty("EdoImportCandidateExistingPurchases")]
    public virtual PurDoc? ExistingPurchase { get; set; }

    [ForeignKey("ImportedPurchaseId")]
    [InverseProperty("EdoImportCandidateImportedPurchase")]
    public virtual PurDoc? ImportedPurchase { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("SelectedContractId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual CmnContract? SelectedContract { get; set; }

    [ForeignKey("SelectedCounterpartyId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual CounterpartyCard? SelectedCounterparty { get; set; }

    [ForeignKey("SelectedCurrencyId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual CmnCurrency? SelectedCurrency { get; set; }

    [ForeignKey("SelectedWarehouseId")]
    [InverseProperty("EdoImportCandidates")]
    public virtual InvWarehouse? SelectedWarehouse { get; set; }
}
