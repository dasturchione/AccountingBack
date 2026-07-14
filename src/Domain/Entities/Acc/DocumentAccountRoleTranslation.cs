using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("DocumentAccountRoleId", "LanguageId")]
[Table("acc_document_account_role_translation")]
public partial class DocumentAccountRoleTranslation
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
    [InverseProperty("DocumentAccountRoleTranslations")]
    public virtual DocumentAccountRole DocumentAccountRole { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.DocumentAccountRoleTranslations))]
    public virtual Language Language { get; set; } = null!;
}
