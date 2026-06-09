using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysModule
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int SubGroupId { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual CmnState State { get; set; } = null!;

    public virtual SysModuleSubGroup SubGroup { get; set; } = null!;

    public virtual ICollection<SysRoleModule> SysRoleModules { get; set; } = new List<SysRoleModule>();
}
