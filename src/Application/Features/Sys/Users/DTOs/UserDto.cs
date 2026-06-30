namespace Application.Features.Users;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int RoleId { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? LastLoginIp { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public string? Timezone { get; set; }
    public DateTime? LastAccessTime { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string RoleName { get; set; } = null!;
    public bool HasGlobalAccess { get; set; }
    public string StateName { get; set; } = null!;
    public List<UserOrganizationItemDto> Organizations { get; set; } = [];
}

public class UserOrganizationItemDto
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsOwner { get; set; }
    public DateTime JoinedAt { get; set; }
    public int? InvitedByUserId { get; set; }
    public DateTime? LastAccessAt { get; set; }
    public DateTime? BlockedAt { get; set; }
}
