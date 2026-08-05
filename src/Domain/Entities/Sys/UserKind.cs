using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("sys_user_kind")]
public partial class UserKind
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [InverseProperty(nameof(UserKindTranslation.UserKind))]
    public virtual ICollection<UserKindTranslation> UserKindTranslations { get; set; } = new List<UserKindTranslation>();

    [InverseProperty(nameof(User.UserKind))]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
