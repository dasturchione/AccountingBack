using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ModuleAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissionCodes;

    public ModuleAuthorizeAttribute(params string[] permissionCodes)
    {
        _permissionCodes = permissionCodes ?? Array.Empty<string>();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_permissionCodes.Length == 0)
            return;

        var userContext        = context.HttpContext.RequestServices.GetService<IUserContext>();
        var permissionChecker  = context.HttpContext.RequestServices.GetService<IPermissionChecker>();

        if (userContext == null || permissionChecker == null || userContext.RoleId is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var hasPermission = await permissionChecker.HasAnyPermissionAsync(
            userContext.RoleId.Value,
            _permissionCodes,
            context.HttpContext.RequestAborted);

        if (!hasPermission)
            context.Result = new ForbidResult();
    }
}
