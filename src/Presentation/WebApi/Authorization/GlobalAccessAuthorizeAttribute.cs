using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class GlobalAccessAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!AuthorizationGuard.EnsureAuthenticated(context))
            return Task.CompletedTask;

        var userContext = context.HttpContext.RequestServices.GetService<IUserContext>();
        if (!AuthorizationGuard.IsSuperAdmin(userContext))
            context.Result = new ForbidResult();

        return Task.CompletedTask;
    }
}
