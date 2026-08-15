using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("edo_import_candidate_marking")]
public sealed class EdoImportCandidateMarking
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("candidate_line_id")]
    public long CandidateLineId { get; set; }

    [Column("marking_number")]
    [StringLength(500)]
    public string MarkingNumber { get; set; } = null!;

    [Column("serial_number")]
    [StringLength(250)]
    public string? SerialNumber { get; set; }

    [Column("provider_verification_state")]
    [StringLength(30)]
    public string ProviderVerificationState { get; set; } = EdoImportMarkingVerificationState.Unverified;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    public EdoImportCandidateLine CandidateLine { get; set; } = null!;
}
