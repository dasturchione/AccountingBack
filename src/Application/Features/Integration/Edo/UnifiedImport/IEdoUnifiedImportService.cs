using SharedKernel.Results;

namespace Application.Features.Integration.Edo.UnifiedImport;

public interface IEdoUnifiedImportService
{
    Task<Result<EdoUnifiedImportPlanDto>> GetPlanAsync(
        IReadOnlyCollection<string>? providerDocumentIds = null,
        bool allowSentDocuments = false,
        CancellationToken ct = default,
        bool allowUnmatchedMarkings = false);
    Task<Result<EdoUnifiedImportApplyResponseDto>> ApplyBatchAsync(EdoUnifiedImportApplyRequestDto request, CancellationToken ct = default);
    Task<Result<EdoUnifiedImportBatchDto>> GetBatchAsync(long batchId, CancellationToken ct = default);
    Task<Result<EdoUnifiedImportBatchDto>> RefreshStatusAsync(CancellationToken ct = default);
}
