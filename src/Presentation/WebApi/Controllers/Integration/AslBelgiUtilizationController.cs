using Application.Features.Integration.AslBelgi.Utilizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Integration;

[Route("api/integrations/asl-belgi/utilizations")]
[ApiController]
[Authorize]
public sealed class AslBelgiUtilizationController(IAslBelgiUtilizationService service) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateUtilization([FromBody] MarkingUtilizationCreateRequestDto request, CancellationToken ct = default)
        => Results.Ok(await service.CreateUtilizationAsync(request, ct));
}
