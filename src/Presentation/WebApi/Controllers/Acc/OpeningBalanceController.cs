using Application.Features.Acc.OpeningBalances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/opening-balances")]
[ApiController]
[Authorize]
public class OpeningBalanceController : ControllerBase
{
    private readonly IOpeningBalanceService _service;
    public OpeningBalanceController(IOpeningBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceView)]
    public async Task<IResult> GetAsync(CancellationToken ct = default)
    {
        var result = await _service.GetAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/accounts/{accountId:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceViewDetail)]
    public async Task<IResult> GetDetailAsync([FromRoute] long id, [FromRoute] long accountId, CancellationToken ct = default)
    {
        var result = await _service.GetDetailAsync(id, accountId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceCreate)]
    public async Task<IResult> CreateAsync([FromBody] OpeningBalanceCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] OpeningBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/accounts")]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceUpdate)]
    public async Task<IResult> SaveAccountAsync([FromRoute] long id, [FromBody] OpeningBalanceAccountSaveDto dto, CancellationToken ct = default)
    {
        var result = await _service.SaveAccountAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningBalanceDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
