using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("UserKindId", "LanguageId")]
[Table("sys_user_kind_translation")]
[Index("Name", Name = "sys_user_kind_translation_name_key", IsUnique = true)]
public partial class SysUserKindTranslation
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
    [InverseProperty("SysUserKindTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("UserKindId")]
    [InverseProperty("SysUserKindTranslations")]
    public virtual SysUserKind UserKind { get; set; } = null!;
}
