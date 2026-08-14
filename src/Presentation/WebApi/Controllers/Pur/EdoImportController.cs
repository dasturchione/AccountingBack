using Application.Features.PurchaseDocs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/purchase-docs/edo-imports")]
[ApiController]
[Authorize]
public sealed class EdoImportController(IEdoImportPreflightService service) : ControllerBase
{
    [HttpPost("preflight")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> StartPreflightAsync(
        [FromBody] EdoImportPreflightRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.StartAsync(request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetJobAsync([FromRoute] long jobId, CancellationToken ct = default)
    {
        var result = await service.GetJobAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/documents")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetDocumentsAsync(
        [FromRoute] long jobId,
        [FromQuery] EdoImportCandidateListFilter filter,
        CancellationToken ct = default)
    {
        var result = await service.GetCandidatesAsync(jobId, filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/documents/{candidateId:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocViewDetail)]
    public async Task<IResult> GetDocumentAsync(
        [FromRoute] long jobId,
        [FromRoute] long candidateId,
        CancellationToken ct = default)
    {
        var result = await service.GetCandidateAsync(jobId, candidateId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/mapping-summary")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetMappingSummaryAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetMappingSummaryAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/master-data-plan")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetMasterDataPlanAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetMasterDataPlanAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/master-data/apply")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyMasterDataAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportMasterDataApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyMasterDataAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/master-data/products/apply-defaults")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyProductDefaultsAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportProductDefaultsApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyProductDefaultsAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/product-conflicts")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetProductConflictsAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetProductConflictsAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/product-conflicts/apply")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyProductConflictsAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportProductConflictApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyProductConflictsAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/marking-conflicts")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetMarkingConflictsAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetMarkingConflictsAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/marking-conflicts/apply")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyMarkingConflictsAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportMarkingConflictApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyMarkingConflictsAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/import-plan")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetImportPlanAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetImportPlanAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/import-drafts")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ImportDraftsAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportDraftBatchRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ImportDraftsAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/bulk-import/start")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> StartBulkImportAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportBulkDraftStartRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.StartBulkImportAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/bulk-import/status")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetBulkImportStatusAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetBulkImportStatusAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/bulk-import/cancel")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> CancelBulkImportAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.CancelBulkImportAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/draft-import-failures")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetDraftImportFailuresAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetDraftImportFailuresAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/draft-import-failures/apply")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyDraftImportFailuresAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportDraftFailureApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyDraftImportFailuresAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/documents/{candidateId:long}/requeue-draft-import")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> RequeueDraftCandidateAsync(
        [FromRoute] long jobId,
        [FromRoute] long candidateId,
        [FromBody] EdoImportDraftRequeueRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.RequeueDraftCandidateAsync(jobId, candidateId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{jobId:long}/piece-tracking-plan")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocView)]
    public async Task<IResult> GetPieceTrackingPlanAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.GetPieceTrackingPlanAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/piece-tracking/apply")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ApplyPieceTrackingAsync(
        [FromRoute] long jobId,
        [FromBody] EdoImportPieceTrackingApplyRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.ApplyPieceTrackingAsync(jobId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{jobId:long}/documents/{candidateId:long}/mapping")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> UpdateDocumentMappingAsync(
        [FromRoute] long jobId,
        [FromRoute] long candidateId,
        [FromBody] EdoImportCandidateMappingRequestDto request,
        CancellationToken ct = default)
    {
        var result = await service.UpdateCandidateMappingAsync(jobId, candidateId, request, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/resolve-mappings")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> ResolveMappingsAsync(
        [FromRoute] long jobId,
        CancellationToken ct = default)
    {
        var result = await service.ResolveMappingsAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("{jobId:long}/cancel")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseDocCreate)]
    public async Task<IResult> CancelAsync([FromRoute] long jobId, CancellationToken ct = default)
    {
        var result = await service.CancelAsync(jobId, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
