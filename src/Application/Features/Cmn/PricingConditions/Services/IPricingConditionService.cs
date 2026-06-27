using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PricingConditions;

public interface IPricingConditionService
{
    Task<Result<PagedResponse<PricingConditionListDto>>> GetAllAsync(PricingConditionListFilter filter, CancellationToken ct = default);
    Task<Result<PricingConditionDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PricingConditionCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<PricingConditionDto>> GetNowAsync(CancellationToken ct = default);
}
