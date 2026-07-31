using Application.Features.Integration.AslBelgi.Aggregations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi/aggregations")]
[ApiController]
[Authorize]
public sealed class AslBelgiAggregationController(IAslBelgiAggregationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateAggregation([FromBody] MarkingAggregationCreateRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CreateAggregationAsync(request, ct));
}
