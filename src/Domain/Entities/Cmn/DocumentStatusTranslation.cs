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
    public short DocumentStatusId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("DocumentStatusId")]
    [InverseProperty(nameof(Entities.DocumentStatus.DocumentStatusTranslations))]
    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.DocumentStatusTranslations))]
    public virtual Language Language { get; set; } = null!;
}
