using Application.Features.SaleDocs;
using Application.Features.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/sale-docs")]
[ApiController]
[Authorize]
public class SaleDocController : ControllerBase
{
    private readonly ISaleDocService _service;
    private readonly IDocumentPdfService _documentPdfService;

    public SaleDocController(ISaleDocService service, IDocumentPdfService documentPdfService)
    {
        _service = service;
        _documentPdfService = documentPdfService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.SaleDocView)]
    public async Task<IResult> GetAllAsync([FromQuery] SaleDocListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/pdf")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocViewDetail)]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IResult> GetPdfAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _documentPdfService.GetSalePdfAsync(id, ct);
        return result.Match(
            file => Results.File(file.Content, file.ContentType, file.FileName),
            CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.SaleDocCreate)]
    public async Task<IResult> CreateAsync([FromBody] SaleDocCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] SaleDocUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/assembly")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocAssembly)]
    public async Task<IResult> AssemblyAsync([FromRoute] long id, [FromBody] List<SaleDocProductAssemblyDto> dtos, CancellationToken ct = default)
    {
        var result = await _service.AssemblyAsync(id, dtos, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("available-products")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocAssembly)]
    public async Task<IResult> GetAvailableProductsAsync([FromQuery] int productId, [FromQuery] int warehouseId, CancellationToken ct = default)
    {
        var result = await _service.GetAvailableProductsAsync(productId, warehouseId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}/available-products")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocAssembly)]
    public async Task<IResult> GetAvailableProductsAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetAvailableProductsAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}/confirm")]
    [ModuleAuthorize(PermissionCodeConst.ConfirmSale)]
    public async Task<IResult> ConfirmAsync([FromRoute] long id, [FromBody] SaleDocConfirmDto dto, CancellationToken ct = default)
    {
        var result = await _service.ConfirmAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPut("{id:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.CancelSale)]
    public async Task<IResult> CancelAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
