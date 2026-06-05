using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysRole
{
    public int Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CmnState State { get; set; } = null!;

    public virtual ICollection<SysRoleModule> SysRoleModules { get; set; } = new List<SysRoleModule>();

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
