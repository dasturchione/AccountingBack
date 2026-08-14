using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("edo_import_candidate_marking")]
[Index("ProviderVerificationState", Name = "idx_edo_import_candidate_marking_verification")]
[Index("CandidateLineId", "MarkingNumber", Name = "ux_edo_import_candidate_marking_line_number", IsUnique = true)]
public partial class EdoImportCandidateMarking
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
    public string ProviderVerificationState { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("CandidateLineId")]
    [InverseProperty("EdoImportCandidateMarkings")]
    public virtual EdoImportCandidateLine CandidateLine { get; set; } = null!;
}
