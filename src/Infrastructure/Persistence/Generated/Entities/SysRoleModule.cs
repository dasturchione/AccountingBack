using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysRoleModule
{
    public int RoleId { get; set; }

    public int ModuleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual SysModule Module { get; set; } = null!;

    public virtual SysRole Role { get; set; } = null!;
}
