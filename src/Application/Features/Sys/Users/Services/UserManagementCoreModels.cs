namespace Application.Features.Users.Services;

public enum UserManagementScope
{
    Organization = 0,
    Global = 1
}

public sealed class UserManagementOptions
{
    public required UserManagementScope Scope { get; init; }
    public bool SendWelcomeEmail { get; init; }

    public static UserManagementOptions ForOrganization(bool sendWelcomeEmail) =>
        new()
        {
            Scope = UserManagementScope.Organization,
            SendWelcomeEmail = sendWelcomeEmail
        };

    public static UserManagementOptions ForGlobal() =>
        new()
        {
            Scope = UserManagementScope.Global,
            SendWelcomeEmail = false
        };
}

public sealed class UserManagementMembershipRequest
{
    public int OrganizationId { get; init; }
    public int? RoleId { get; init; }
    public bool IsDefault { get; init; }
    public bool IsOwner { get; init; }
    public int? InvitedByUserId { get; init; }
}

public sealed class UserManagementCreateRequest
{
    public string UserName { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? Email { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public int TenantId { get; init; }
    public short UserKindId { get; init; }
    public short? LanguageId { get; init; }
    public bool EmailVerified { get; init; }
    public string? Timezone { get; init; }
    public List<UserManagementMembershipRequest> Organizations { get; init; } = [];
}

public sealed class UserManagementUpdateRequest
{
    public int UserId { get; init; }
    public string UserName { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public string? Email { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public short UserKindId { get; init; }
    public short? LanguageId { get; init; }
    public bool EmailVerified { get; init; }
    public string? Timezone { get; init; }
    public short StateId { get; init; }
    public List<UserManagementMembershipRequest>? Organizations { get; init; }
}

public sealed class UserWelcomeEmailMessage
{
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}

public sealed class UserManagementCreateResult
{
    public int UserId { get; init; }
    public UserWelcomeEmailMessage? WelcomeEmail { get; init; }
}