using Application.Features.CounterpartyRegisterBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IResult> GetAllAsync([FromQuery] CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    public async Task<IResult> CreateAsync([FromBody] CounterpartyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] CounterpartyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
