using Application.Features.Inv.OpeningInventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/opening-inventories")]
[ApiController]
[Authorize]
public sealed class OpeningInventoryController : ControllerBase
{
    private readonly IOpeningInventoryService _service;

    public OpeningInventoryController(IOpeningInventoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.OpeningInventoryView)]
    public async Task<IResult> GetAllAsync(
        [FromQuery] OpeningInventoryListFilter filter,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningInventoryViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.OpeningInventoryCreate)]
    public async Task<IResult> CreateAsync(
        [FromBody] OpeningInventoryCreateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningInventoryUpdate)]
    public async Task<IResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] OpeningInventoryUpdateDto dto,
        CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.OpeningInventoryDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
