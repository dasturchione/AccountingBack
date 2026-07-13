using Application.Features.Edocs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

[Route("api/edocs")]
[ApiController]
[Authorize]
public sealed class EdocsController(IEdocsIntegrationService service) : ControllerBase
{
    [HttpPost("auth/challenge")]
    public async Task<IResult> CreateChallengeAsync([FromBody] EdocsChallengeRequest request, CancellationToken ct = default) =>
        (await service.CreateChallengeAsync(request, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("auth/login")]
    public async Task<IResult> LoginAsync([FromBody] EdocsLoginRequest request, CancellationToken ct = default) =>
        (await service.LoginAsync(request, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("profile")]
    public async Task<IResult> GetProfileAsync(CancellationToken ct = default) =>
        (await service.GetProfileAsync(ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents")]
    public async Task<IResult> GetDocumentsAsync([FromQuery] EdocsDocumentListQuery query, CancellationToken ct = default) =>
        (await service.GetDocumentsAsync(query, ct)).Match(Results.Ok, CustomResults.Problem);
}
