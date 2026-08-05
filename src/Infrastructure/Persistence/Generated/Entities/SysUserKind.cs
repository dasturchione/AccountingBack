using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_user_kind")]
[Index("Code", Name = "sys_user_kind_code_key", IsUnique = true)]
[Index("Name", Name = "sys_user_kind_name_key", IsUnique = true)]
public partial class SysUserKind
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [InverseProperty("UserKind")]
    public virtual ICollection<SysUserKindTranslation> SysUserKindTranslations { get; set; } = new List<SysUserKindTranslation>();

    [InverseProperty("UserKind")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
