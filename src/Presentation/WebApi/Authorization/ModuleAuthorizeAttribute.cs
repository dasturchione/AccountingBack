using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;

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
        if (!AuthorizationGuard.EnsureAuthenticated(context))
            return;

        if (_permissionCodes.Length == 0)
            return;

        var userContext = context.HttpContext.RequestServices.GetService<IUserContext>();
        if (AuthorizationGuard.HasGlobalAccess(userContext))
        {
            await TryWriteGlobalAccessAuditAsync(context, userContext!);
            return;
        }

        var permissionChecker = context.HttpContext.RequestServices.GetService<IPermissionChecker>();

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

    private async Task TryWriteGlobalAccessAuditAsync(AuthorizationFilterContext context, IUserContext userContext)
    {
        var auditLogService = context.HttpContext.RequestServices.GetService<IAuditLogService>();
        var logger = context.HttpContext.RequestServices.GetService<ILogger<ModuleAuthorizeAttribute>>();

        if (auditLogService == null)
            return;

        try
        {
            var recordId = userContext.Id?.ToString() ?? context.HttpContext.TraceIdentifier;

            auditLogService.SetNewValues(new
            {
                userId = userContext.Id,
                method = context.HttpContext.Request.Method,
                endpoint = context.HttpContext.Request.Path.Value ?? "/",
                permissionCodes = _permissionCodes,
                result = "GLOBAL_ACCESS_BYPASS_GRANTED",
                traceId = context.HttpContext.TraceIdentifier
            });

            await auditLogService.CreateAsync(
                AuditLogTableConst.AuthorizationBypass,
                recordId,
                AuditLogOperationTypeConst.Update);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(
                ex,
                "Failed to persist global access bypass audit for {Method} {Path}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path.Value ?? "/");
        }
    }
}
