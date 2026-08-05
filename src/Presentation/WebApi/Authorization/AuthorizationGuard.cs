using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

internal static class AuthorizationGuard
{
    public static bool EnsureAuthenticated(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
            return true;

        context.Result = new UnauthorizedResult();
        return false;
    }

    public static bool IsSuperAdmin(IUserContext? userContext) =>
        userContext?.UserKind == CurrentUserKind.SuperAdmin;
}
