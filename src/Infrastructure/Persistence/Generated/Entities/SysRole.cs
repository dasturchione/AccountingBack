using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_role")]
[Index("Code", Name = "idx_sys_role_code")]
[Index("IsSystem", Name = "idx_sys_role_is_system")]
[Index("OrganizationId", Name = "idx_sys_role_organization_id")]
[Index("SortOrder", Name = "idx_sys_role_sort_order")]
public partial class SysRole
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("short_name")]
    [StringLength(100)]
    public string ShortName { get; set; } = null!;

    [Column("full_name")]
    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    [Column("has_global_access")]
    public bool HasGlobalAccess { get; set; }

    [Column("code")]
    [StringLength(100)]
    public string? Code { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("is_system")]
    public bool IsSystem { get; set; }

    [Column("is_owner_role")]
    public bool IsOwnerRole { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("SysRoles")]
    public virtual OrgOrganization? Organization { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("SysRoles")]
    public virtual CmnState State { get; set; } = null!;

    [InverseProperty("Role")]
    public virtual ICollection<SysRoleModule> SysRoleModules { get; set; } = new List<SysRoleModule>();

    [InverseProperty("Role")]
    public virtual ICollection<SysUserOrganization> SysUserOrganizations { get; set; } = new List<SysUserOrganization>();

    [InverseProperty("Role")]
    public virtual ICollection<SysUser> SysUsers { get; set; } = new List<SysUser>();
}
