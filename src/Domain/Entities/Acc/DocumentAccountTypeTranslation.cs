using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("LanguageId", "DocumentAccountTypeId")]
[Table("acc_document_account_type_translation")]
public partial class DocumentAccountTypeTranslation
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
    [InverseProperty(nameof(DocumentAccountType.DocumentAccountTypeTranslations))]
    public virtual DocumentAccountType DocumentAccountType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.DocumentAccountTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
