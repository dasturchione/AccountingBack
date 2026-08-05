using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Security.Claims;

namespace WebApi.Middlewares;

public class OrganizationScopeMiddleware
{
    internal const string AllowedOrgIdsKey = "AllowedOrgIds";
    public const string CurrentOrgIdKey = "CurrentOrgId";
    public const string CurrentRoleIdKey = "CurrentRoleId";
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
                var userKindId = await db.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(user => user.Id == userId && user.StateId == StateIdConst.ACTIVE)
                    .Select(user => (short?)user.UserKindId)
                    .SingleOrDefaultAsync(context.RequestAborted);
                var hasGlobalAccess = userKindId == UserKindIdConst.SuperAdmin;

                var memberships = await db.UserOrganizations
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(membership => membership.UserId == userId && membership.StateId == StateIdConst.ACTIVE)
                    .Select(membership => new { membership.OrganizationId, membership.RoleId })
                    .ToListAsync(context.RequestAborted);

                var allowedOrgIds = memberships.Select(membership => membership.OrganizationId).ToList();
                context.Items[AllowedOrgIdsKey] = allowedOrgIds;
                context.Items[TrustedGlobalAccessKey] = hasGlobalAccess;

                if (currentOrgId.HasValue)
                {
                    var membership = memberships.FirstOrDefault(item => item.OrganizationId == currentOrgId.Value);
                    if (!hasGlobalAccess && membership is null)
                    {
                        await WriteScopeErrorAsync(context, StatusCodes.Status403Forbidden, "OrganizationMembershipRequired");
                        return;
                    }

                    if (membership?.RoleId is int roleId && roleId > 0)
                        context.Items[CurrentRoleIdKey] = roleId;

                    context.Items[CurrentOrgIdKey] = currentOrgId.Value;
                }
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