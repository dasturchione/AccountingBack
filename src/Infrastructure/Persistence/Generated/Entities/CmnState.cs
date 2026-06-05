using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class CmnState
{
    public short Id { get; set; }

    public string ShortName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CmnDistrict> CmnDistricts { get; set; } = new List<CmnDistrict>();

    public virtual ICollection<CmnRegion> CmnRegions { get; set; } = new List<CmnRegion>();

    public virtual ICollection<SysModule> SysModules { get; set; } = new List<SysModule>();

    public virtual ICollection<SysRole> SysRoles { get; set; } = new List<SysRole>();

    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
