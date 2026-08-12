namespace Application.Features.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = default!;
    public UserResponseDto User { get; set; } = default!;
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int TenantId { get; set; }
    public short UserKindId { get; set; }
    public string UserKindCode { get; set; } = null!;
    public DateTime? LastAccessTime { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string StateName { get; set; } = null!;
    public List<UserOrgDto> Organizations { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}

public class UserOrgDto
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = null!;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool IsDefault { get; set; }
}