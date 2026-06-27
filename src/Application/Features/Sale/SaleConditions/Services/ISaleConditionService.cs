using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.SaleConditions;

public interface ISaleConditionService
{
    Task<Result<PagedResponse<SaleConditionListDto>>> GetAllAsync(SaleConditionListFilter filter, CancellationToken ct = default);
    Task<Result<SaleConditionDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(SaleConditionCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<SaleConditionDto>> GetNowAsync(CancellationToken ct = default);
}
