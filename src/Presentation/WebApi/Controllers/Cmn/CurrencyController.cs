using Application.Features.Cmn.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/currencies")]
[ApiController]
[Authorize]
public sealed class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _service;

    public CurrencyController(ICurrencyService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CurrencyView)]
    public async Task<IResult> GetAll([FromQuery] CurrencyListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyViewDetail)]
    public async Task<IResult> GetById([FromRoute] short id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CurrencyCreate)]
    public async Task<IResult> Create([FromBody] CurrencyCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyUpdate)]
    public async Task<IResult> Update([FromRoute] short id, [FromBody] CurrencyUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id}")]
    [ModuleAuthorize(PermissionCodeConst.CurrencyDelete)]
    public async Task<IResult> Delete([FromRoute] short id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
