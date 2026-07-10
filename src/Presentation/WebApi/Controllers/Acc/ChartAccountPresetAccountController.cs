using Application.Features.ChartAccountPresetAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/chart-account-preset-accounts")]
[ApiController]
//[Authorize]
public class ChartAccountPresetAccountController : ControllerBase
{
    private readonly IChartAccountPresetAccountService _service;

    public ChartAccountPresetAccountController(IChartAccountPresetAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountView)]
    //[GlobalAccessAuthorize]
    public async Task<IResult> GetAllAsync([FromQuery] ChartAccountPresetAccountListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("grouped")]
    //[ModuleAuthorize(PermissionCodeConst.ChartAccountView)]
    //[GlobalAccessAuthorize]
    public async Task<IResult> GetGroupedAsync([FromQuery] ChartAccountPresetAccountListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetGroupedListAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
