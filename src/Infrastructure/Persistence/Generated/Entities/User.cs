using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("sys_user")]
[Index("LanguageId", Name = "idx_sys_user_language_id")]
[Index("OrganizationId", Name = "idx_sys_user_organization_id")]
[Index("PhoneNumber", Name = "idx_sys_user_phone")]
[Index("RoleId", Name = "idx_sys_user_role_id")]
[Index("UserName", Name = "uidx_sys_user_user_name", IsUnique = true)]
[Index("Email", Name = "idx_sys_user_email")]
[Index("EmailVerified", Name = "idx_sys_user_email_verified")]
[Index("IsPlatformAdmin", Name = "idx_sys_user_is_platform_admin")]
public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_name")]
    [StringLength(250)]
    public string UserName { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(250)]
    public string PasswordHash { get; set; } = null!;

    [Column("password_salt")]
    [StringLength(250)]
    public string PasswordSalt { get; set; } = null!;

    [Column("phone_number")]
    [StringLength(50)]
    public string PhoneNumber { get; set; } = null!;

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("first_name")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("last_access_time", TypeName = "timestamp without time zone")]
    public DateTime? LastAccessTime { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("language_id")]
    public short? LanguageId { get; set; }

    [Column("organization_id")]
    public int? OrganizationId { get; set; }


    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("email_verified_at", TypeName = "timestamp without time zone")]
    public DateTime? EmailVerifiedAt { get; set; }

    [Column("last_login_ip")]
    [StringLength(64)]
    public string? LastLoginIp { get; set; }

    [Column("is_platform_admin")]
    public bool IsPlatformAdmin { get; set; }

    [Column("timezone")]
    [StringLength(100)]
    public string? Timezone { get; set; }
    [InverseProperty("ResponsibleUser")]
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

    [ForeignKey("LanguageId")]
    [InverseProperty("Users")]
    public virtual Language? Language { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("Users")]
    public virtual Organization? Organization { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;

    [ForeignKey("StateId")]
    [InverseProperty("Users")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
}
