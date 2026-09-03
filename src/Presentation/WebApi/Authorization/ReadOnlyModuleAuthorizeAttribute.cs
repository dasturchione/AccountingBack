using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ReadOnlyModuleAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissionCodes;

    public ReadOnlyModuleAuthorizeAttribute(params string[] permissionCodes) =>
        _permissionCodes = permissionCodes ?? Array.Empty<string>();

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!AuthorizationGuard.EnsureAuthenticated(context))
            return;

        if (_permissionCodes.Length == 0)
            return;

        var userContext = context.HttpContext.RequestServices.GetService<IUserContext>();
        if (userContext?.UserKind == CurrentUserKind.SuperAdmin)
            return;

        if (userContext?.RoleId is not int roleId || roleId <= 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissionChecker = context.HttpContext.RequestServices.GetService<IPermissionChecker>();
        if (permissionChecker is null || !await permissionChecker.HasAnyPermissionAsync(
                roleId,
                _permissionCodes,
                context.HttpContext.RequestAborted))
            context.Result = new ForbidResult();
    }
}
