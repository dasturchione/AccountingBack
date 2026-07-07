using Application.Features.FaDepreciations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/fa/depreciation")]
[ApiController]
[Authorize]
public class FaDepreciationController : ControllerBase
{
    private readonly IFaDepreciationRunService _service;

    public FaDepreciationController(IFaDepreciationRunService service)
    {
        _service = service;
    }

    [HttpGet("run")]
    [ModuleAuthorize(PermissionCodeConst.FaDepreciationView)]
    public async Task<IResult> GetAllAsync([FromQuery] FaDepreciationRunListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("run/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.FaDepreciationViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("run")]
    [ModuleAuthorize(PermissionCodeConst.FaDepreciationRun)]
    public async Task<IResult> RunAsync([FromQuery] string period, CancellationToken ct = default)
    {
        var result = await _service.RunAsync(period, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("run/{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelFaDepreciation)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
