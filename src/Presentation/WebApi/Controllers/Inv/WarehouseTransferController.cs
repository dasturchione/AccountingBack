using Application.Features.WarehouseTransfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/warehouse-transfers")]
[ApiController]
[Authorize]
public class WarehouseTransferController : ControllerBase
{
    private readonly IWarehouseTransferService _service;

    public WarehouseTransferController(IWarehouseTransferService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferView)]
    public async Task<IResult> GetAllAsync([FromQuery] WarehouseTransferListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferCreate)]
    public async Task<IResult> CreateAsync([FromBody] WarehouseTransferCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] WarehouseTransferUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmWarehouseTransfer)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelWarehouseTransfer)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{id:long}/posting-batches")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferViewDetail)]
    public async Task<IResult> GetPostingBatchesAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetPostingBatchesAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/inventory-movements")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferViewDetail)]
    public async Task<IResult> GetInventoryMovementsAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetInventoryMovementsAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
