using Application.Features.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Sys;

[Route("api/dashboard")]
[ApiController]
[Authorize]
[GlobalAccessAuthorize]
[ModuleAuthorize(PermissionCodeConst.DashboardView)]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IResult> GetStatsAsync(CancellationToken ct = default)
    {
        var result = await _dashboardService.GetStatsAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
