using Application.Features.TrialBalance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/trial-balance")]
[ApiController]
[Authorize]
public class TrialBalanceController : ControllerBase
{
    private readonly ITrialBalanceService _service;

    public TrialBalanceController(ITrialBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetAsync([FromQuery] TrialBalanceFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
