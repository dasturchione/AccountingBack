using Application.Features.CashCollections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/cash-collection-docs")]
[ApiController]
[Authorize]
public sealed class CashCollectionController : ControllerBase
{
    private readonly ICashCollectionService _service;

    public CashCollectionController(ICashCollectionService service) => _service = service;

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionView)]
    public async Task<IResult> GetAllAsync([FromQuery] CashCollectionListFilter filter, CancellationToken ct)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("in-transit")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionView)]
    public async Task<IResult> GetInTransitAsync([FromQuery] int? bankAccountId, CancellationToken ct)
    {
        var result = await _service.GetInTransitAsync(bankAccountId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionViewDetail)]
    public async Task<IResult> GetByIdAsync(long id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionCreate)]
    public async Task<IResult> CreateAsync([FromBody] CashCollectionCreateDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionUpdate)]
    public async Task<IResult> UpdateAsync(long id, [FromBody] CashCollectionUpdateDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionDelete)]
    public async Task<IResult> DeleteAsync(long id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/send-to-bank")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionSendToBank)]
    public async Task<IResult> SendToBankAsync(long id, CancellationToken ct)
    {
        var result = await _service.SendToBankAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CashCollectionCancel)]
    public async Task<IResult> CancelAsync(long id, CancellationToken ct)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
