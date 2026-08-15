using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_candidate_line")]
public sealed class EdoImportCandidateLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("candidate_id")]
    public long CandidateId { get; set; }

    [Column("provider_line_number")]
    public int ProviderLineNumber { get; set; }

    [Column("catalog_code")]
    [StringLength(50)]
    public string? CatalogCode { get; set; }

    [Column("provider_product_name")]
    [StringLength(500)]
    public string? ProviderProductName { get; set; }

    [Column("package_code")]
    [StringLength(50)]
    public string? PackageCode { get; set; }

    [Column("package_name")]
    [StringLength(500)]
    public string? PackageName { get; set; }

    [Column("is_service")]
    public bool? IsService { get; set; }

    [Column("quantity")]
    [Precision(19, 6)]
    public decimal? Quantity { get; set; }

    [Column("unit_price")]
    [Precision(24, 8)]
    public decimal? UnitPrice { get; set; }

    [Column("net_amount")]
    [Precision(24, 8)]
    public decimal? NetAmount { get; set; }

    [Column("vat_rate")]
    [Precision(9, 4)]
    public decimal? VatRate { get; set; }

    [Column("vat_amount")]
    [Precision(24, 8)]
    public decimal? VatAmount { get; set; }

    [Column("total_amount")]
    [Precision(24, 8)]
    public decimal? TotalAmount { get; set; }

    [Column("selected_product_id")]
    public int? SelectedProductId { get; set; }

    [Column("selected_unit_id")]
    public short? SelectedUnitId { get; set; }

    [Column("selected_vat_rate_id")]
    public short? SelectedVatRateId { get; set; }

    [Column("selected_debit_account_id")]
    public int? SelectedDebitAccountId { get; set; }

    [Column("selected_vat_account_id")]
    public int? SelectedVatAccountId { get; set; }

    [Column("mapping_status")]
    [StringLength(30)]
    public string MappingStatus { get; set; } = EdoImportMappingStatus.Unresolved;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    public EdoImportCandidate Candidate { get; set; } = null!;

    public Product? SelectedProduct { get; set; }

    public Unit? SelectedUnit { get; set; }

    public VatRate? SelectedVatRate { get; set; }

    public ChartAccount? SelectedDebitAccount { get; set; }

    public ChartAccount? SelectedVatAccount { get; set; }

    public ICollection<EdoImportCandidateMarking> Markings { get; } = new List<EdoImportCandidateMarking>();
}
