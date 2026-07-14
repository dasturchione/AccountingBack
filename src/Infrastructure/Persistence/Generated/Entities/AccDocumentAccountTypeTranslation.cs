using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("LanguageId", "DocumentAccountTypeId")]
[Table("acc_document_account_type_translation")]
public partial class AccDocumentAccountTypeTranslation
{
    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Key]
    [Column("document_account_type_id")]
    public short DocumentAccountTypeId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [ForeignKey("DocumentAccountTypeId")]
    [InverseProperty("AccDocumentAccountTypeTranslations")]
    public virtual AccDocumentAccountType DocumentAccountType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccDocumentAccountTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
