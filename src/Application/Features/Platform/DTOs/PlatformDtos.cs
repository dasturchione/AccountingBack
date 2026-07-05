using Application.Features.AuditLogs;
using Application.Features.Organizations;
using Application.Features.OrganizationSetup;
using Application.Features.Users;
using SharedKernel.Filters;

namespace Application.Features.Platform;

public sealed class PlatformTenantListFilter : IPaginationFilter
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public short? StateId { get; set; }
}

public sealed class PlatformDashboardDto : DashboardStatsDto
{
}

public class PlatformTenantBaseDto
{
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public int? OwnerUserId { get; set; }
}

public sealed class PlatformTenantCreateDto : PlatformTenantBaseDto;

public sealed class PlatformTenantUpdateDto : PlatformTenantBaseDto
{
    public short StateId { get; set; }
}

public class PlatformTenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int? OwnerUserId { get; set; }
    public string? OwnerUserName { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int OrganizationsCount { get; set; }
    public int UsersCount { get; set; }
}

public sealed class PlatformTenantDetailDto : PlatformTenantDto
{
    public List<PlatformOrganizationItemDto> Organizations { get; set; } = [];
}

public sealed class PlatformUserListFilter : IPaginationFilter
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public int? RoleId { get; set; }
    public short? StateId { get; set; }
    public int? OrganizationId { get; set; }
    public int? TenantId { get; set; }
    public bool? HasGlobalAccess { get; set; }
}

public sealed class PlatformOrganizationListFilter : IPaginationFilter
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public int? TenantId { get; set; }
    public int? RegionId { get; set; }
    public short? StateId { get; set; }
    public string? SetupStatus { get; set; }
}

public sealed class PlatformAuditLogListFilter : IPaginationFilter
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; } = 50;
    public int? OrganizationId { get; set; }
    public int? UserId { get; set; }
    public int? ChangedUserId { get; set; }
    public string? EntityType { get; set; }
    public string? TableName { get; set; }
    public string? EntityId { get; set; }
    public string? RecordId { get; set; }
    public string? Action { get; set; }
    public string? SearchText { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
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
    public PlatformWorkspaceSetupDto? Setup { get; set; }
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
    public int? TenantId { get; set; }
    public string? SetupStatus { get; set; }
    public DateTime? SetupCompletedAt { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Oked { get; set; }
    public short StateId { get; set; }
}

public sealed class AccountantWorkspaceCreateDto
{
    public string TenantName { get; set; } = null!;
    public string? TenantSlug { get; set; }

    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int RoleId { get; set; }
    public short? LanguageId { get; set; }
    public string? Timezone { get; set; }

    public string OrganizationShortName { get; set; } = null!;
    public string OrganizationFullName { get; set; } = null!;
    public string Inn { get; set; } = null!;
    public string? OrganizationPhoneNumber { get; set; }
    public int RegionId { get; set; }
    public int? DistrictId { get; set; }
    public string? Address { get; set; }
    public string? Director { get; set; }
    public short? DefaultLanguageId { get; set; }
    public string? OrganizationEmail { get; set; }
    public string? Website { get; set; }
    public string? Oked { get; set; }

    public short? TaxTypeId { get; set; }
    public bool IsVatPayer { get; set; }
    public string? VatRegistrationNumber { get; set; }
    public DateOnly? TaxEffectiveFrom { get; set; }
    public DateOnly? TaxEffectiveTo { get; set; }

    public short? AccountingPolicyId { get; set; }
    public short? BaseCurrencyId { get; set; }
    public DateOnly? AccountingStartDate { get; set; }
    public string? InventoryValuationMethod { get; set; }
    public short FiscalYearStartMonth { get; set; } = 1;

    public OrganizationSetupDefaultsDto? Defaults { get; set; }
    public bool CompleteSetup { get; set; }
}

public sealed class AccountantWorkspaceDto
{
    public PlatformTenantDto Tenant { get; set; } = null!;
    public PlatformOrganizationItemDto Organization { get; set; } = null!;
    public PlatformUserDto User { get; set; } = null!;
    public PlatformUserOrganizationDto Membership { get; set; } = null!;
    public PlatformWorkspaceSetupDto Setup { get; set; } = null!;
}

public class PlatformUserDto : UserListDto
{
    public int OrganizationsCount { get; set; }
}

public sealed class PlatformUserDetailDto : PlatformUserDto
{
    public List<PlatformUserOrganizationDto> Organizations { get; set; } = [];
}

public sealed class PlatformUserCreateDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int RoleId { get; set; }
    public short? LanguageId { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public string? Timezone { get; set; }
    public List<PlatformUserOrganizationCreateDto> Organizations { get; set; } = [];
}

public sealed class PlatformUserUpdateDto
{
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public int RoleId { get; set; }
    public short? LanguageId { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public string? Timezone { get; set; }
    public short StateId { get; set; }
    public List<PlatformUserOrganizationCreateDto>? Organizations { get; set; }
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

public sealed class PlatformWorkspaceSetupDto
{
    public string CurrentStep { get; set; } = null!;
    public bool OrganizationCompleted { get; set; }
    public bool TaxCompleted { get; set; }
    public bool AccountingCompleted { get; set; }
    public bool DefaultsCompleted { get; set; }
    public bool UsersCompleted { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
