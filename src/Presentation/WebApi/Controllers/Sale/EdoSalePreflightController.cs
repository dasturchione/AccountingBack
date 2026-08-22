using Application.Features.SaleDocs;
using Application.Features.SaleDocs.EdoSalePreflight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/sale-docs/edo-outbox")]
[ApiController]
[Authorize]
public sealed class EdoSalePreflightController(
    IEdoSalePreflightService service,
    IEdoSaleDraftApplyService applyService) : ControllerBase
{
    [HttpGet("plan")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocView)]
    [ProducesResponseType(typeof(EdoSalePreflightPlanDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetPlan(CancellationToken ct = default) =>
        Results.Ok(await service.GetPlanAsync(ct));

    [HttpPost("apply")]
    [ModuleAuthorize(PermissionCodeConst.SaleDocCreate)]
    [ProducesResponseType(typeof(EdoSaleDraftApplyResponseDto), StatusCodes.Status200OK)]
    public async Task<IResult> Apply(
        [FromBody] EdoSaleDraftApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await applyService.ApplyAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
