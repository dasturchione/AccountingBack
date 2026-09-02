using Application.Abstractions.Authentication;

namespace IntegrationTests.Infrastructure;

public sealed class IntegrationTestUserContext : IUserContext
{
    public int? Id { get; set; }

    public int? RoleId { get; set; }

    public CurrentUserKind UserKind { get; set; } = CurrentUserKind.TenantUser;

    public short? LanguageId { get; set; }

    public int? TenantId { get; set; }

    public int? OrganizationId { get; set; }

    public List<int> AllowedOrganizationIds { get; set; } = [];

    public int? BranchId { get; set; }
}
