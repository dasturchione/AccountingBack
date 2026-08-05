namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    int? Id { get; }

    int? RoleId { get; }

    short? UserKindId { get; }

    short? LanguageId { get; }

    int? TenantId { get; }

    int? OrganizationId { get; }

    List<int> AllowedOrganizationIds { get; }

    int? BranchId { get; }

    bool HasGlobalAccess { get; }
}