using Application.Features.CashOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/cash-operations")]
[ApiController]
[Authorize]
public class CashOperationController : ControllerBase
{
    private readonly ICashOperationService _service;

    public CashOperationController(ICashOperationService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CashOperationView)]
    public async Task<IResult> GetAllAsync([FromQuery] CashOperationListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CashOperationCreate)]
    public async Task<IResult> CreateAsync([FromBody] CashOperationCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmCashOperation)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelCashOperation)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] CashOperationUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
