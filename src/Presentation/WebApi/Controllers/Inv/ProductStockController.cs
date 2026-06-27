using Application.Features.Inv.ProductStocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/product-stocks")]
[ApiController]
[Authorize]
public class ProductStockController : ControllerBase
{
    private readonly IProductStockService _service;
    public ProductStockController(IProductStockService service)
    {
        _service = service;
    }

    [HttpGet("by-marking")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetByMarkingNumber([FromQuery] string markingNumber, CancellationToken ct = default)
    {
        markingNumber = Uri.UnescapeDataString(markingNumber);
        var result = await _service.GetByMarkingNumberAsync(markingNumber, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("groups")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductGroupSummary([FromQuery] ProductGroupStockFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductGroupsStockAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("products")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductSummary([FromQuery] ProductStockFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductsStockAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tables")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductTableSummary([FromQuery] ProductTableStockFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductTablesStockAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{productId:int}/purchases")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetPurchasesByProductId([FromRoute] int productId, CancellationToken ct = default)
    {
        var result = await _service.GetPurchasesByProductIdAsync(productId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
