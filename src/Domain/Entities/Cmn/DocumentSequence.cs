using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_document_sequence")]
[Index("DocumentTypeId", Name = "idx_cmn_document_sequence_document_type_id")]
[Index("StateId", Name = "idx_cmn_document_sequence_state_id")]
public partial class DocumentSequence
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("prefix")]
    [StringLength(50)]
    public string? Prefix { get; set; }

    [Column("suffix")]
    [StringLength(50)]
    public string? Suffix { get; set; }

    [Column("current_number")]
    public long CurrentNumber { get; set; }

    [Column("padding")]
    public short Padding { get; set; }

    [Column("year")]
    public short? Year { get; set; }

    [Column("month")]
    public short? Month { get; set; }

    [Column("reset_period")]
    [StringLength(20)]
    public string ResetPeriod { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
