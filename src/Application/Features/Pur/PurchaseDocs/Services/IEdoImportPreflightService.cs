using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocs;

public interface IEdoImportPreflightService
{
    Task<Result<EdoImportJobDto>> StartAsync(EdoImportPreflightRequestDto request, CancellationToken ct = default);
    Task<Result<EdoImportJobDto>> GetJobAsync(long jobId, CancellationToken ct = default);
    Task<Result<PagedResponse<EdoImportCandidateListDto>>> GetCandidatesAsync(long jobId, EdoImportCandidateListFilter filter, CancellationToken ct = default);
    Task<Result<EdoImportCandidateDetailDto>> GetCandidateAsync(long jobId, long candidateId, CancellationToken ct = default);
    Task<Result<EdoImportMappingSummaryDto>> GetMappingSummaryAsync(long jobId, CancellationToken ct = default);
    Task<Result<EdoImportMasterDataPlanDto>> GetMasterDataPlanAsync(long jobId, CancellationToken ct = default);
    Task<Result<EdoImportMasterDataApplyResponseDto>> ApplyMasterDataAsync(
        long jobId,
        EdoImportMasterDataApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportMasterDataApplyResponseDto>> ApplyProductDefaultsAsync(
        long jobId,
        EdoImportProductDefaultsApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportProductConflictPlanDto>> GetProductConflictsAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportProductConflictApplyResponseDto>> ApplyProductConflictsAsync(
        long jobId,
        EdoImportProductConflictApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportMarkingConflictPlanDto>> GetMarkingConflictsAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportMarkingConflictApplyResponseDto>> ApplyMarkingConflictsAsync(
        long jobId,
        EdoImportMarkingConflictApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportDraftPlanDto>> GetImportPlanAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportDraftBatchResponseDto>> ImportDraftsAsync(
        long jobId,
        EdoImportDraftBatchRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportBulkDraftStatusDto>> StartBulkImportAsync(
        long jobId,
        EdoImportBulkDraftStartRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportBulkDraftStatusDto>> GetBulkImportStatusAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportBulkDraftStatusDto>> CancelBulkImportAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportDraftFailureListDto>> GetDraftImportFailuresAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportDraftFailureApplyResponseDto>> ApplyDraftImportFailuresAsync(
        long jobId,
        EdoImportDraftFailureApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportDraftRequeueResponseDto>> RequeueDraftCandidateAsync(
        long jobId,
        long candidateId,
        EdoImportDraftRequeueRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportPieceTrackingPlanDto>> GetPieceTrackingPlanAsync(
        long jobId,
        CancellationToken ct = default);
    Task<Result<EdoImportPieceTrackingApplyResponseDto>> ApplyPieceTrackingAsync(
        long jobId,
        EdoImportPieceTrackingApplyRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportCandidateDetailDto>> UpdateCandidateMappingAsync(
        long jobId,
        long candidateId,
        EdoImportCandidateMappingRequestDto request,
        CancellationToken ct = default);
    Task<Result<EdoImportJobDto>> ResolveMappingsAsync(long jobId, CancellationToken ct = default);
    Task<Result<EdoImportJobDto>> CancelAsync(long jobId, CancellationToken ct = default);
}

public interface IEdoBulkDraftImportProcessor
{
    Task ProcessBulkImportAsync(long jobId, string workerId, CancellationToken ct = default);
    Task<IReadOnlyCollection<long>> GetRunnableBulkImportJobIdsAsync(CancellationToken ct = default);
}

public interface IEdoImportPreflightProcessor
{
    Task ProcessAsync(long jobId, string leaseOwner, CancellationToken ct = default);
    Task<IReadOnlyCollection<long>> GetRunnableJobIdsAsync(CancellationToken ct = default);
}
