using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("DocumentTypeId", "LanguageId")]
[Table("cmn_document_type_translation")]
[Index("LanguageId", Name = "ix_cmn_document_type_translation_language_id")]
public partial class CmnDocumentTypeTranslation
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
    [InverseProperty("CmnDocumentTypeTranslations")]
    public virtual CmnDocumentType DocumentType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnDocumentTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
