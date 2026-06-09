namespace Domain.Entities;

public partial class User
{
    public int Id { get; set; }
    public int? OrganizationId { get; set; }
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int RoleId { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public short? LanguageId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual Organization? Organization { get; set; }
    public virtual Role Role { get; set; } = null!;
    public virtual State State { get; set; } = null!;
}
