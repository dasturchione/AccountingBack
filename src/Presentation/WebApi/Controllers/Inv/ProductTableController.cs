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

    [HttpGet("by-marking/{**markingNumber}")]
    [ModuleAuthorize(PermissionCodeConst.ProductTableView)]
    public async Task<IResult> GetByMarkingNumber([FromRoute] string markingNumber, CancellationToken ct = default)
    {
        markingNumber = Uri.UnescapeDataString(markingNumber);
        var result = await _service.GetByMarkingNumberAsync(markingNumber, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
