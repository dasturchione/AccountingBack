using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_module")]
[Index("Code", Name = "sys_module_unique_index_code", IsUnique = true)]
[Index("SubGroupId", Name = "sys_module_unique_index_sub_group_id")]
public partial class SysModule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string Code { get; set; } = null!;

    [Column("short_name")]
    [StringLength(250)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(300)]
    public string FullName { get; set; } = null!;

    [Column("sub_group_id")]
    public int SubGroupId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("SysModules")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("SubGroupId")]
    [InverseProperty("SysModules")]
    public virtual SysModuleSubGroup SubGroup { get; set; } = null!;

    [InverseProperty("Module")]
    public virtual ICollection<SysRoleModule> SysRoleModules { get; set; } = new List<SysRoleModule>();
}
