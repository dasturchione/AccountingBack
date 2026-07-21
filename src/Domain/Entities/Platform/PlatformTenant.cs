using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("platform_tenant")]
[Index("OwnerUserId", Name = "idx_platform_tenant_owner_user_id")]
[Index("StateId", Name = "idx_platform_tenant_state_id")]
[Index("Slug", Name = "platform_tenant_slug_key", IsUnique = true)]
public partial class PlatformTenant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("slug")]
    [StringLength(150)]
    public string Slug { get; set; } = null!;

    [Column("owner_user_id")]
    public int? OwnerUserId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [InverseProperty(nameof(Organization.PlatformTenant))]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    [InverseProperty(nameof(User.PlatformTenant))]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
