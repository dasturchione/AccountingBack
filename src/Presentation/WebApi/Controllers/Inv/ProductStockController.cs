using Application.Features.Inv.ProductStocks;
using Application.Features.Inv.WarehouseProducts;
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
    private readonly IWarehouseInventoryService _warehouseInventoryService;

    public ProductStockController(
        IProductStockService service,
        IWarehouseInventoryService warehouseInventoryService)
    {
        _service = service;
        _warehouseInventoryService = warehouseInventoryService;
    }

    [HttpGet("by-marking")]
    [ModuleAuthorize(PermissionCodeConst.ProductStockGetByMarkingNumber)]
    public async Task<IResult> GetByMarkingNumber([FromQuery] string markingNumber, CancellationToken ct = default)
    {
        markingNumber = Uri.UnescapeDataString(markingNumber);
        var result = await _service.GetByMarkingNumberAsync(markingNumber, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("groups")]
    [ModuleAuthorize(PermissionCodeConst.ProductStockGetProductGroupSummary)]
    public async Task<IResult> GetProductGroupSummary([FromQuery] ProductGroupStockFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductGroupsStockAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("products")]
    [ModuleAuthorize(PermissionCodeConst.ProductStockGetProductSummary)]
    public async Task<IResult> GetProductSummary([FromQuery] WarehouseProductFilter filter, CancellationToken ct = default)
    {
        var result = await _warehouseInventoryService.GetWarehouseProductsAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tables")]
    [ModuleAuthorize(PermissionCodeConst.ProductStockGetProductTableSummary)]
    public async Task<IResult> GetProductTableSummary([FromQuery] ProductTableStockFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductTablesStockAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
