using Application.Features.InventoryCounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/inventory-counts")]
[ApiController]
[Authorize]
public class InventoryCountController : ControllerBase
{
    private readonly IInventoryCountService _service;

    public InventoryCountController(IInventoryCountService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountView)]
    public async Task<IResult> GetAllAsync([FromQuery] InventoryCountListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountCreate)]
    public async Task<IResult> CreateAsync([FromBody] InventoryCountCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] InventoryCountUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmInventoryCount)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelInventoryCount)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{id:long}/posting-batches")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountGetPostingBatches)]
    public async Task<IResult> GetPostingBatchesAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetPostingBatchesAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/inventory-movements")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountGetInventoryMovements)]
    public async Task<IResult> GetInventoryMovementsAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetInventoryMovementsAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/differences")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountGetDifferences)]
    public async Task<IResult> GetDifferencesAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetDifferencesAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
