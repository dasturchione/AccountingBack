using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Security.Claims;

namespace WebApi.Middlewares;

public class OrganizationScopeMiddleware
{
    internal const string AllowedOrgIdsKey = "AllowedOrgIds";
    public const string CurrentOrgIdKey = "CurrentOrgId";
    public const string TrustedGlobalAccessKey = "TrustedGlobalAccess";

    private readonly RequestDelegate _next;

    public OrganizationScopeMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db, Application.Abstractions.Authentication.IUserContext userContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Items[AllowedOrgIdsKey] = new List<int>();
            context.Items[TrustedGlobalAccessKey] = false;

            if (int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) && userId > 0)
            {
                var roleId = ParsePositiveInt(context.User.FindFirst(ClaimTypes.Role)?.Value);
                var claimOrgId = ParsePositiveInt(context.User.FindFirst("OrganizationId")?.Value);
                var headerOrgId = ParseHeader(context, out var invalidHeader);

                if (invalidHeader)
                {
                    await WriteScopeErrorAsync(context, StatusCodes.Status400BadRequest, "InvalidOrganizationScope");
                    return;
                }

                if (claimOrgId.HasValue && headerOrgId.HasValue && claimOrgId != headerOrgId)
                {
                    await WriteScopeErrorAsync(context, StatusCodes.Status403Forbidden, "OrganizationScopeConflict");
                    return;
                }

                var currentOrgId = headerOrgId ?? claimOrgId;
                var hasGlobalAccess = roleId.HasValue && await db.Roles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(role => role.Id == roleId.Value
                        && role.StateId == StateIdConst.ACTIVE
                        && role.OrganizationId == null
                        && role.HasGlobalAccess, context.RequestAborted);

                var allowedOrgIds = await db.UserOrganizations
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(uo => uo.UserId == userId && uo.StateId == StateIdConst.ACTIVE)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync(context.RequestAborted);

                context.Items[AllowedOrgIdsKey] = allowedOrgIds;
                context.Items[TrustedGlobalAccessKey] = hasGlobalAccess;

                if (currentOrgId.HasValue && !hasGlobalAccess && !allowedOrgIds.Contains(currentOrgId.Value))
                {
                    await WriteScopeErrorAsync(context, StatusCodes.Status403Forbidden, "OrganizationMembershipRequired");
                    return;
                }

                if (currentOrgId.HasValue)
                    context.Items[CurrentOrgIdKey] = currentOrgId.Value;
            }
        }

        db.SetUserContext(userContext);
        await _next(context);
    }

    private static int? ParsePositiveInt(string? value) =>
        int.TryParse(value, out var id) && id > 0 ? id : null;

    private static int? ParseHeader(HttpContext context, out bool invalid)
    {
        invalid = false;
        var value = context.Request.Headers["X-OrganizationId"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!int.TryParse(value, out var id) || id <= 0)
        {
            invalid = true;
            return null;
        }

        return id;
    }

    private static Task WriteScopeErrorAsync(HttpContext context, int statusCode, string safeErrorCode)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            Ready = false,
            CredentialPresent = false,
            SessionPresent = false,
            ContractState = "Blocked",
            SafeErrorCode = safeErrorCode
        });
    }
}
