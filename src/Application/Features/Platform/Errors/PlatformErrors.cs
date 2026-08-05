using SharedKernel.Results;

namespace Application.Features.Platform;

public static class PlatformErrors
{
    public static Error GlobalAccessRequired() =>
        Error.Forbidden("Platform.GlobalAccessRequired", "Only users with global access can use platform administration endpoints.");

    public static Error TenantNotFound(int id) =>
        Error.NotFound("PlatformTenant.NotFound", $"Tenant with id '{id}' was not found.");

    public static Error TenantSlugConflict(string slug) =>
        Error.Conflict("PlatformTenant.SlugConflict", $"Tenant with slug '{slug}' already exists.");

    public static Error UserNotFound(int id) =>
        Error.NotFound("PlatformUser.NotFound", $"User with id '{id}' was not found.");

    public static Error UserNameConflict(string userName) =>
        Error.Conflict("PlatformUser.UserNameConflict", $"User with username '{userName}' already exists.");

    public static Error RoleNotFound(int id) =>
        Error.NotFound("PlatformRole.NotFound", $"Role with id '{id}' was not found.");

    public static Error UserKindNotFound(short id) =>
        Error.NotFound("PlatformUserKind.NotFound", $"User kind with id '{id}' was not found.");

    public static Error OrganizationNotFound(int id) =>
        Error.NotFound("PlatformOrganization.NotFound", $"Organization with id '{id}' was not found.");

    public static Error OrganizationInnConflict(string inn) =>
        Error.Conflict("PlatformOrganization.InnConflict", $"Organization with INN '{inn}' already exists.");

    public static Error UserOrganizationConflict(int userId, int organizationId) =>
        Error.Conflict("PlatformUserOrganization.Conflict", $"User {userId} is already attached to organization {organizationId}.");

    public static Error UserOrganizationNotFound(int userId, int organizationId) =>
        Error.NotFound("PlatformUserOrganization.NotFound", $"User {userId} is not attached to organization {organizationId}.");

    public static Error InvalidInventoryValuationMethod(string method) =>
        Error.Business("Platform.InvalidInventoryValuationMethod", $"Inventory valuation method '{method}' is not supported.");
}