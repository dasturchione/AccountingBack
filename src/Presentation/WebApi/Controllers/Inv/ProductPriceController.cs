using Application.Features.Inv.ProductPrices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/product-prices")]
[ApiController]
[Authorize]
public class ProductPriceController : ControllerBase
{
    private readonly IProductPriceService _service;

    public ProductPriceController(IProductPriceService service)
    {
        _service = service;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceView)]
    public async Task<IResult> GetAllAsync([FromQuery] ProductPriceListFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{productId:int}/details")]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceGetPriceDetailsByProductId)]
    public async Task<IResult> GetPriceDetailsByProductIdAsync([FromRoute] int productId, CancellationToken ct = default)
    {
        var result = await _service.GetPriceDetailsByProductIdAsync(productId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceCreate)]
    public async Task<IResult> CreateAsync([FromBody] ProductPriceCreateDto dto, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(dto, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] long id, [FromBody] ProductPriceUpdateDto dto, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, dto, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.ProductPriceDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] long id, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(id, ct);
        return result.Match(Results.NoContent, CustomResults.Problem);
    }
}
