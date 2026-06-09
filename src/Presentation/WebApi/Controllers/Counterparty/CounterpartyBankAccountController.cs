using Application.Features.CounterpartyBankAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/counterparty-bank-accounts")]
[ApiController]
[Authorize]
public class CounterpartyBankAccountController : ControllerBase
{
    private readonly ICounterpartyBankAccountService _service;

    public CounterpartyBankAccountController(ICounterpartyBankAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyBankAccountView)]
    public async Task<IResult> GetAll([FromQuery] CounterpartyBankAccountListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyBankAccountViewDetail)]
    public async Task<IResult> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyBankAccountCreate)]
    public async Task<IResult> Create([FromBody] CounterpartyBankAccountCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyBankAccountUpdate)]
    public async Task<IResult> Update([FromRoute] int id, [FromBody] CounterpartyBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyBankAccountDelete)]
    public async Task<IResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
