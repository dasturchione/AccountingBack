using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_document_number_sequence")]
public partial class DocumentNumberSequence
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Column("document_year")]
    public short DocumentYear { get; set; }

    [Column("prefix")]
    [StringLength(20)]
    public string? Prefix { get; set; }

    [Column("last_number")]
    public long LastNumber { get; set; }

    [Column("number_length")]
    public short? NumberLength { get; set; }

    [Column("last_document_date", TypeName = "timestamp without time zone")]
    public DateTime LastDocumentDate { get; set; }

    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "timestamp without time zone")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("DocumentTypeId")]
    [InverseProperty(nameof(DocumentType.DocumentNumberSequences))]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty(nameof(Organization.DocumentNumberSequences))]
    public virtual Organization Organization { get; set; } = null!;
}
