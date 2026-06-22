using Application.Features.ProductTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/product-tables")]
[ApiController]
[Authorize]
public class ProductTableController : ControllerBase
{
    private readonly IProductTableService _service;

    public ProductTableController(IProductTableService service)
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

    [HttpGet("product-group-summary")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductGroupSummary(CancellationToken ct = default)
    {
        var result = await _service.GetProductGroupSummaryAsync(ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("product-summary")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductSummary([FromQuery] ProductTableGroupFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetProductSummaryAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("product-table-summary")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetProductTableSummary([FromQuery] int? productGroupId, [FromQuery] int? productId, CancellationToken ct = default)
    {
        var result = await _service.GetProductTableSummaryAsync(productGroupId, productId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
