using Application.Features.InventoryAdjustments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/inventory-adjustments")]
[ApiController]
[Authorize]
public class InventoryAdjustmentController : ControllerBase
{
    private readonly IInventoryAdjustmentService _service;

    public InventoryAdjustmentController(IInventoryAdjustmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentView)]
    public async Task<IResult> GetAllAsync([FromQuery] InventoryAdjustmentListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentCreate)]
    public async Task<IResult> CreateAsync([FromBody] InventoryAdjustmentCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] InventoryAdjustmentUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmInventoryAdjustment)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelInventoryAdjustment)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("{id:long}/posting-batches")]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentViewDetail)]
    public async Task<IResult> GetPostingBatchesAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetPostingBatchesAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/inventory-movements")]
    [ModuleAuthorize(PermissionCodeConst.InventoryAdjustmentViewDetail)]
    public async Task<IResult> GetInventoryMovementsAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetInventoryMovementsAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
