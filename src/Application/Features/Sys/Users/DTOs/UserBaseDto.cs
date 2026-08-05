namespace Application.Features.Users;

public class UserBaseDto
{
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public string? Timezone { get; set; }
    public List<UserOrganizationRequestDto> Organizations { get; set; } = [];
}

public sealed class UserOrganizationRequestDto
{
    public int OrganizationId { get; set; }
    public int? RoleId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsOwner { get; set; }
    public int? InvitedByUserId { get; set; }
}