using Application.Features.MoneyRegisterBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/register/money-register-balances")]
[ApiController]
[Authorize]
public class MoneyRegisterBalanceController : ControllerBase
{
    private readonly IMoneyRegisterBalanceService _service;

    public MoneyRegisterBalanceController(IMoneyRegisterBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.MoneyRegBalanceView)]
    public async Task<IResult> GetAllAsync([FromQuery] MoneyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.MoneyRegBalanceViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.MoneyRegBalanceCreate)]
    public IResult CreateAsync([FromBody] MoneyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Money register balances are read-only and can only be changed by document posting.");
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.MoneyRegBalanceUpdate)]
    public IResult UpdateAsync([FromRoute] long id, [FromBody] MoneyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Money register balances are read-only and can only be changed by document posting.");
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.MoneyRegBalanceDelete)]
    public IResult DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status405MethodNotAllowed,
            title: "Method Not Allowed",
            detail: "Money register balances are read-only and can only be changed by document posting.");
    }
}
