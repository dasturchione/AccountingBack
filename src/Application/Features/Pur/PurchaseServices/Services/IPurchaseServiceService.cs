using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PurchaseServices;

public interface IPurchaseServiceService
{
    Task<Result<PagedResponse<PurchaseServiceListDto>>> GetAllAsync(PurchaseServiceListFilter filter, CancellationToken ct = default);
    Task<Result<PurchaseServiceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PurchaseServiceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PurchaseServiceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
