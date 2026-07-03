using Application.Features.Acc.AccountingPeriods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/accounting-periods")]
[ApiController]
[Authorize]
public class AccountingPeriodController : ControllerBase
{
    private readonly IAccountingPeriodService _service;

    public AccountingPeriodController(IAccountingPeriodService service)
    {
        _service = service;
    }

    [HttpPost("{id:int}/close")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryUpdate)]
    public async Task<IResult> CloseAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.CloseAsync(id, ct);
        return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result);
    }

    [HttpPost("{id:int}/reopen")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryUpdate)]
    public async Task<IResult> ReopenAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.ReopenAsync(id, ct);
        return result.IsSuccess ? Results.Ok() : CustomResults.Problem(result);
    }
}
