using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("DocumentStatusId", "LanguageId")]
[Table("cmn_document_status_translation")]
public partial class DocumentStatusTranslation
{
    [Key]
    [Column("document_status_id")]
    public short StatusId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("DocumentStatusId")]
    [InverseProperty(nameof(DocumentStatus.DocumentStatusTranslations))]
    public virtual DocumentStatus Status { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.DocumentStatusTranslations))]
    public virtual Language Language { get; set; } = null!;
}
