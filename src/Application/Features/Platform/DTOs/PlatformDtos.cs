using Application.Features.AuditLogs;
using Application.Features.Organizations;
using Application.Features.Users;

namespace Application.Features.Platform;

public sealed class PlatformDashboardDto : DashboardStatsDto
{
}

public sealed class PlatformOrganizationItemDto
{
    public int Id { get; set; }
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string SetupStatus { get; set; } = null!;
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class PlatformOrganizationDto : OrganizationDto
{
    public string? TenantName { get; set; }
    public int UsersCount { get; set; }
}

public sealed class PlatformOrganizationDetailDto : PlatformOrganizationDto
{
    public List<PlatformUserOrganizationDto> Users { get; set; } = [];
}

public sealed class PlatformOrganizationUpdateDto
{
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public bool IsParent { get; set; }
    public short? DefaultLanguageId { get; set; }
    public int TenantId { get; set; }
    public string? SetupStatus { get; set; }
    public DateTime? SetupCompletedAt { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Oked { get; set; }
    public short StateId { get; set; }
}

public class PlatformUserDto : UserListDto
{
    public int OrganizationsCount { get; set; }
}

public sealed class PlatformUserDetailDto : PlatformUserDto
{
    public List<PlatformUserOrganizationDto> Organizations { get; set; } = [];
}

public sealed class PlatformUserOrganizationCreateDto
{
    public int OrganizationId { get; set; }
    public int? RoleId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsOwner { get; set; }
    public int? InvitedByUserId { get; set; }
}

public sealed class PlatformUserOrganizationUpdateDto
{
    public int? RoleId { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsOwner { get; set; }
    public short? StateId { get; set; }
    public bool? IsBlocked { get; set; }
}

public sealed class PlatformUserOrganizationDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsOwner { get; set; }
    public short StateId { get; set; }
    public DateTime JoinedAt { get; set; }
    public int? InvitedByUserId { get; set; }
    public DateTime? LastAccessAt { get; set; }
    public DateTime? BlockedAt { get; set; }
}

public sealed class PlatformAuditLogDto : AuditLogCoreDto
{
    public string? OrganizationName { get; set; }
    public string? RequestId { get; set; }
    public string? ClientAddr { get; set; }
    public string? ApplicationName { get; set; }
}

public sealed class PlatformSetPasswordDto
{
    public string Password { get; set; } = null!;
}
