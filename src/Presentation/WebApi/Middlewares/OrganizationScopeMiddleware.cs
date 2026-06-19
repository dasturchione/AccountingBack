using Application.Abstractions.Authentication;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using System.Security.Claims;

namespace WebApi.Middlewares
{
    public class OrganizationScopeMiddleware
    {
        private const string AllowedOrgIdsKey = "AllowedOrgIds";

        private readonly RequestDelegate _next;

        public OrganizationScopeMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, AppDbContext db, IUserContext userContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdStr, out var userId))
                {
                    var hasGlobalAccess = context.User.FindFirst("HasGlobalAccess")?.Value == "true";

                    if (hasGlobalAccess)
                    {
                        context.Items[AllowedOrgIdsKey] = new List<int>();
                    }
                    else
                    {
                        var allowedOrgIds = await db.UserOrganizations
                            .Where(uo => uo.UserId == userId && uo.StateId == StateIdConst.ACTIVE)
                            .Select(uo => uo.OrganizationId)
                            .ToListAsync();

                        context.Items[AllowedOrgIdsKey] = allowedOrgIds;

                        var headerVal = context.Request.Headers["X-OrganizationId"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(headerVal) && int.TryParse(headerVal, out var requestedOrgId))
                        {
                            if (!allowedOrgIds.Contains(requestedOrgId))
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    type   = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                                    title  = "Forbidden",
                                    status = 403,
                                    detail = "You do not have access to this organization."
                                });
                                return;
                            }
                        }
                    }
                }
            }

            // DbContext ga UserContext ni beramiz — query filters ishlashi uchun
            db.SetUserContext(userContext);

            await _next(context);
        }
    }
}
