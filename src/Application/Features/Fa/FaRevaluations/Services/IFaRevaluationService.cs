using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.FaRevaluations;

public interface IFaRevaluationService
{
    Task<Result<PagedResponse<FaRevaluationListDto>>> GetAllAsync(FaRevaluationListFilter filter, CancellationToken ct = default);
    Task<Result<FaRevaluationDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(FaRevaluationCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, FaRevaluationUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
