using Application.Features.Ledger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/ledger")]
[ApiController]
[Authorize]
public class LedgerController : ControllerBase
{
    private readonly ILedgerService _service;

    public LedgerController(ILedgerService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetAsync([FromQuery] LedgerFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
