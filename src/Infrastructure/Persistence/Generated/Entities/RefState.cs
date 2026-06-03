using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class RefState
{
    public short Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<RefDistrict> RefDistricts { get; set; } = new List<RefDistrict>();

    public virtual ICollection<RefRegion> RefRegions { get; set; } = new List<RefRegion>();

    public virtual ICollection<SysModule> SysModules { get; set; } = new List<SysModule>();

    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
