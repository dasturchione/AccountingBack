using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("DocumentAccountRoleId", "LanguageId")]
[Table("acc_document_account_role_translation")]
public partial class AccDocumentAccountRoleTranslation
{
    [Key]
    [Column("document_account_role_id")]
    public short DocumentAccountRoleId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [ForeignKey("DocumentAccountRoleId")]
    [InverseProperty("AccDocumentAccountRoleTranslations")]
    public virtual AccDocumentAccountRole DocumentAccountRole { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccDocumentAccountRoleTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
