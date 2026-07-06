using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("UserId", "OrganizationId")]
[Table("sys_user_organization")]
[Index("InvitedByUserId", Name = "idx_sys_user_organization_invited_by_user_id")]
[Index("IsOwner", Name = "idx_sys_user_organization_is_owner")]
[Index("OrganizationId", Name = "idx_sys_user_organization_organization_id")]
[Index("RoleId", Name = "idx_sys_user_organization_role_id")]
[Index("StateId", Name = "idx_sys_user_organization_state_id")]
public partial class SysUserOrganization
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Key]
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("is_owner")]
    public bool IsOwner { get; set; }

    [Column("joined_at", TypeName = "timestamp without time zone")]
    public DateTime JoinedAt { get; set; }

    [Column("invited_by_user_id")]
    public int? InvitedByUserId { get; set; }

    [Column("last_access_at", TypeName = "timestamp without time zone")]
    public DateTime? LastAccessAt { get; set; }

    [Column("blocked_at", TypeName = "timestamp without time zone")]
    public DateTime? BlockedAt { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("SysUserOrganizations")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("SysUserOrganizations")]
    public virtual SysRole? Role { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("SysUserOrganizations")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("SysUserOrganization")]
    public virtual SysUser User { get; set; } = null!;
}
