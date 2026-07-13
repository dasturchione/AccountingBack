using Application.Features.Edocs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Cmn;

// E-DOCS endpoints reuse the existing tax-integration module permissions (same precedent as the
// Asl Belgisi integration): connection state changes require TAX_CREATE, reads require TAX_VIEW.
// Dedicated EDOCS_* permission codes would need a sys_module seed and are tracked separately.
[Route("api/edocs")]
[ApiController]
[Authorize]
public sealed class EdocsController(IEdocsIntegrationService service) : ControllerBase
{
    [HttpPost("auth/challenge")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> CreateChallengeAsync([FromBody] EdocsChallengeRequest request, CancellationToken ct = default) =>
        (await service.CreateChallengeAsync(request, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("auth/login")]
    [ModuleAuthorize(PermissionCodeConst.TaxCreate)]
    public async Task<IResult> LoginAsync([FromBody] EdocsLoginRequest request, CancellationToken ct = default) =>
        (await service.LoginAsync(request, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("profile")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetProfileAsync(CancellationToken ct = default) =>
        (await service.GetProfileAsync(ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents")]
    [ModuleAuthorize(PermissionCodeConst.TaxView)]
    public async Task<IResult> GetDocumentsAsync([FromQuery] EdocsDocumentListQuery query, CancellationToken ct = default) =>
        (await service.GetDocumentsAsync(query, ct)).Match(Results.Ok, CustomResults.Problem);
}
