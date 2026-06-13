using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_role")]
[Index("OrganizationId", Name = "idx_sys_role_organization_id")]
public partial class Role
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

    [ForeignKey("OrganizationId")]
    [InverseProperty("Roles")]
    public virtual Organization? Organization { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("Roles")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("Role")]
    public virtual ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();

    [InverseProperty("Role")]
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();

    [InverseProperty("Role")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
