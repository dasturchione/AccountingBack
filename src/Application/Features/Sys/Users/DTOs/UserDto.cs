namespace Application.Features.Users;

public class UserDto : UserListDto
{
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
