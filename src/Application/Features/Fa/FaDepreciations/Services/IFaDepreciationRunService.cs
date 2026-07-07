using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaDepreciations;

public interface IFaDepreciationRunService
{
    Task<Result<PagedResponse<FaDepreciationRunListDto>>> GetAllAsync(FaDepreciationRunListFilter filter, CancellationToken ct = default);
    Task<Result<FaDepreciationRunDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> RunAsync(string period, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
