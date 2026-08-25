using Application.Features.CashFiscalTransfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/cash-fiscal-transfers")]
[ApiController]
[Authorize]
public sealed class CashFiscalTransferController : ControllerBase
{
    private readonly ICashFiscalTransferService _service;

    public CashFiscalTransferController(ICashFiscalTransferService service) => _service = service;

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CashFiscalTransferView)]
    public async Task<IResult> GetAllAsync([FromQuery] CashFiscalTransferListFilter filter, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashFiscalTransferViewDetail)]
    public async Task<IResult> GetByIdAsync(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CashFiscalTransferCreate)]
    public async Task<IResult> CreateAsync([FromBody] CashFiscalTransferCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashFiscalTransferUpdate)]
    public async Task<IResult> UpdateAsync(long id, [FromBody] CashFiscalTransferUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashFiscalTransferDelete)]
    public async Task<IResult> DeleteAsync(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmCashFiscalTransfer)]
    public async Task<IResult> ConfirmAsync(long id, CancellationToken ct)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelCashFiscalTransfer)]
    public async Task<IResult> CancelAsync(long id, CancellationToken ct)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
