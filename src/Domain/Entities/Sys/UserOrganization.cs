using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey("UserId", "OrganizationId")]
[Table("sys_user_organization")]
[Index("OrganizationId", Name = "idx_sys_user_organization_organization_id")]
[Index("RoleId", Name = "idx_sys_user_organization_role_id")]
[Index("StateId", Name = "idx_sys_user_organization_state_id")]
public partial class UserOrganization
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

    [ForeignKey("OrganizationId")]
    [InverseProperty("UserOrganizations")]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("UserOrganizations")]
    public virtual Role? Role { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("UserOrganizations")]
    public virtual State State { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserOrganizations")]
    public virtual User User { get; set; } = null!;
}
