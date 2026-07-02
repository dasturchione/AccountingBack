using Application.Features.CounterpartyRegisterBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/counterparty-register-balances")]
[ApiController]
[Authorize]
public class CounterpartyRegisterBalanceController : ControllerBase
{
    private readonly ICounterpartyRegisterBalanceService _service;

    public CounterpartyRegisterBalanceController(ICounterpartyRegisterBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceView)]
    public async Task<IResult> GetAllAsync([FromQuery] CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceCreate)]
    public IResult CreateAsync([FromBody] CounterpartyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Counterparty register balances are read-only and can only be changed by document posting.");
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceUpdate)]
    public IResult UpdateAsync([FromRoute] long id, [FromBody] CounterpartyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Counterparty register balances are read-only and can only be changed by document posting.");
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceDelete)]
    public IResult DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Counterparty register balances are read-only and can only be changed by document posting.");
    }
}
