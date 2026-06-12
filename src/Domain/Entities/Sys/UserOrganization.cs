namespace Domain.Entities;

public partial class UserOrganization
{
    public int UserId { get; set; }
    public int OrganizationId { get; set; }
    public int? RoleId { get; set; }
    public bool IsDefault { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization Organization { get; set; } = null!;
    public virtual Role? Role { get; set; }
    public virtual State State { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
