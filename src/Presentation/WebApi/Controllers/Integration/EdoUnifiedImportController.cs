using Application.Features.Integration.Edo.UnifiedImport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Integration;

[Route("api/edo/import")]
[ApiController]
[Authorize]
[Produces("application/json")]
public sealed class EdoUnifiedImportController(IEdoUnifiedImportService service) : ControllerBase
{
    [HttpGet("plan")]
    [ProducesResponseType(typeof(EdoUnifiedImportPlanDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetPlan(
        [FromQuery(Name = "providerDocumentId")] string[]? providerDocumentIds,
        [FromQuery] bool allowSentDocuments = false,
        [FromQuery] bool allowUnmatchedMarkings = false,
        CancellationToken ct = default) =>
        (await service.GetPlanAsync(
            providerDocumentIds,
            allowSentDocuments,
            ct,
            allowUnmatchedMarkings)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("plan")]
    [ProducesResponseType(typeof(EdoUnifiedImportPlanDto), StatusCodes.Status200OK)]
    public async Task<IResult> PostPlan(
        [FromBody] EdoUnifiedImportPlanRequestDto request,
        CancellationToken ct = default) =>
        (await service.GetPlanAsync(
            request.ProviderDocumentIds,
            request.AllowSentDocuments,
            ct,
            request.AllowUnmatchedMarkings)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("apply-batch")]
    [ProducesResponseType(typeof(EdoUnifiedImportApplyResponseDto), StatusCodes.Status200OK)]
    public async Task<IResult> ApplyBatch(
        [FromBody] EdoUnifiedImportApplyRequestDto request,
        CancellationToken ct = default) =>
        (await service.ApplyBatchAsync(request, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("{batchId:long}")]
    [ProducesResponseType(typeof(EdoUnifiedImportBatchDto), StatusCodes.Status200OK)]
    public async Task<IResult> GetBatch(long batchId, CancellationToken ct = default) =>
        (await service.GetBatchAsync(batchId, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpPost("refresh-status")]
    [ProducesResponseType(typeof(EdoUnifiedImportBatchDto), StatusCodes.Status200OK)]
    public async Task<IResult> RefreshStatus(CancellationToken ct = default) =>
        (await service.RefreshStatusAsync(ct)).Match(Results.Ok, CustomResults.Problem);
}
