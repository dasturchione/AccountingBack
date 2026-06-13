using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("RoleId", "ModuleId")]
[Table("sys_role_module")]
public partial class SysRoleModule
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Key]
    [Column("module_id")]
    public int ModuleId { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ModuleId")]
    [InverseProperty("SysRoleModules")]
    public virtual SysModule Module { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("SysRoleModules")]
    public virtual SysRole Role { get; set; } = null!;
}
