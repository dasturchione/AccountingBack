namespace Application.Features.Organizations;

public enum OrganizationManagementScope
{
    Organization = 0,
    Global = 1
}

public sealed class OrganizationManagementOptions
{
    public required OrganizationManagementScope Scope { get; init; }
    public bool TrimInput { get; init; }
    public bool ValidateTenantExists { get; init; }
    public bool IncludeDetails { get; init; }

    public static OrganizationManagementOptions ForOrganization(bool includeDetails = false) =>
        new()
        {
            Scope = OrganizationManagementScope.Organization,
            TrimInput = false,
            ValidateTenantExists = false,
            IncludeDetails = includeDetails
        };

    public static OrganizationManagementOptions ForGlobal(bool includeDetails = false) =>
        new()
        {
            Scope = OrganizationManagementScope.Global,
            TrimInput = true,
            ValidateTenantExists = true,
            IncludeDetails = includeDetails
        };
}

public sealed class OrganizationManagementUpdateRequest
{
    public int OrganizationId { get; init; }
    public string ShortName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Inn { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public int RegionId { get; init; }
    public int? DistrictId { get; init; }
    public string? Address { get; init; }
    public string? Director { get; init; }
    public bool IsParent { get; init; }
    public short? DefaultLanguageId { get; init; }
    public int? TenantId { get; init; }
    public string? SetupStatus { get; init; }
    public DateTime? SetupCompletedAt { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? Oked { get; init; }
    public short StateId { get; init; }
}
