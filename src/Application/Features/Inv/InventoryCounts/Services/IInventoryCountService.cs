using Application.Common.Pagination;
using Application.Features.InventoryMovements;
using SharedKernel.Results;

namespace Application.Features.InventoryCounts;

public interface IInventoryCountService
{
    Task<Result<PagedResponse<InventoryCountListDto>>> GetAllAsync(InventoryCountListFilter filter, CancellationToken ct = default);
    Task<Result<InventoryCountDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(InventoryCountCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, InventoryCountUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryCountPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryMovementListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryCountDifferenceDto>>> GetDifferencesAsync(long id, CancellationToken ct = default);
}
