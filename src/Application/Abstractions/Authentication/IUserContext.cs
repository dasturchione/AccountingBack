using SharedKernel.Constants;

namespace Application.Abstractions.Authentication;

public interface IUserContext
{
    int? Id { get; }

    int? RoleId { get; }

    CurrentUserKind UserKind { get; }

    short? LanguageId { get; }

    int? TenantId { get; }

    int? OrganizationId { get; }

    List<int> AllowedOrganizationIds { get; }

    int? BranchId { get; }
}

public enum CurrentUserKind : short
{
    None = 0,
    SuperAdmin = UserKindIdConst.SuperAdmin,
    TenantAdmin = UserKindIdConst.TenantAdmin,
    TenantUser = UserKindIdConst.TenantUser
}
