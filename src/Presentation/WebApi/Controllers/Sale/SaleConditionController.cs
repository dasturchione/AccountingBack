using Application.Features.SaleConditions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/sale-conditions")]
[ApiController]
[Authorize]
public class SaleConditionController : ControllerBase
{
    private readonly ISaleConditionService _service;

    public SaleConditionController(ISaleConditionService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.SaleConditionView)]
    public async Task<IResult> GetAllAsync([FromQuery] SaleConditionListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("now")]
    [ModuleAuthorize(PermissionCodeConst.SaleConditionView)]
    public async Task<IResult> GetNowAsync(CancellationToken ct = default)
    {
        var result = await _service.GetNowAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleConditionViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.SaleConditionCreate)]
    public async Task<IResult> CreateAsync([FromBody] SaleConditionCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleConditionDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
