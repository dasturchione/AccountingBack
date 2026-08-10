using Application.Features.RetailSaleDocs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/retail-sale-docs")]
[ApiController]
[Authorize]
public class RetailSaleDocController : ControllerBase
{
    private readonly IRetailSaleDocService _service;

    public RetailSaleDocController(IRetailSaleDocService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.SaleDocView)]
    public async Task<IResult> GetAllAsync([FromQuery] RetailSaleDocListFilter filter, CancellationToken ct = default) =>
        (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocViewDetail)]
    public async Task<IResult> GetByIdAsync(long id, CancellationToken ct = default) =>
        (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.SaleDocCreate)]
    public async Task<IResult> CreateAsync([FromBody] RetailSaleDocCreateDto dto, CancellationToken ct = default) =>
        (await _service.CreateAsync(dto, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocUpdate)]
    public async Task<IResult> UpdateAsync(long id, [FromBody] RetailSaleDocUpdateDto dto, CancellationToken ct = default) =>
        (await _service.UpdateAsync(id, dto, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmSale)]
    public async Task<IResult> ConfirmAsync(long id, [FromBody] RetailSaleDocConfirmDto dto, CancellationToken ct = default) =>
        (await _service.ConfirmAsync(id, dto, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelSale)]
    public async Task<IResult> CancelAsync(long id, CancellationToken ct = default) =>
        (await _service.CancelAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocDelete)]
    public async Task<IResult> DeleteAsync(long id, CancellationToken ct = default) =>
        (await _service.DeleteAsync(id, ct)).Match(Results.NoContent, CustomResults.Problem);
}
