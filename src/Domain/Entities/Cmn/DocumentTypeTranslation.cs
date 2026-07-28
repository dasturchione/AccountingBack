using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("DocumentTypeId", "LanguageId")]
[Table("cmn_document_type_translation")]
public partial class DocumentTypeTranslation
{
    [Key]
    [Column("document_type_id")]
    public short DocumentTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty(nameof(Entities.DocumentType.DocumentTypeTranslations))]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.DocumentTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
