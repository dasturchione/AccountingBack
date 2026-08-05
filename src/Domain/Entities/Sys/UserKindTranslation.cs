using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("UserKindId", "LanguageId")]
[Table("sys_user_kind_translation")]
public partial class UserKindTranslation
{
    [Key]
    [Column("user_kind_id")]
    public short UserKindId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.UserKindTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("UserKindId")]
    [InverseProperty(nameof(UserKind.UserKindTranslations))]
    public virtual UserKind UserKind { get; set; } = null!;
}
