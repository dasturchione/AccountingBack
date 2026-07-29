using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("DocumentStatusId", "LanguageId")]
[Table("cmn_document_status_translation")]
[Index("LanguageId", Name = "ix_cmn_document_status_translation_language_id")]
public partial class CmnDocumentStatusTranslation
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
    [InverseProperty("CmnDocumentStatusTranslations")]
    public virtual CmnDocumentStatus DocumentStatus { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnDocumentStatusTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
