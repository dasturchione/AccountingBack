using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Generated.Entities;

public partial class SysUserOrganization
{
    public int UserId { get; set; }

    public int OrganizationId { get; set; }

    public int? RoleId { get; set; }

    public bool IsDefault { get; set; }

    public short StateId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual OrgOrganization Organization { get; set; } = null!;

    public virtual SysRole? Role { get; set; }

    public virtual CmnState State { get; set; } = null!;

    public virtual SysUser User { get; set; } = null!;
}
