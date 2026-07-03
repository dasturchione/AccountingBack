using Application.Common.Pagination;
using Application.Features.InventoryRegisterBalances;
using SharedKernel.Results;

namespace Application.Features.InventoryAdjustments;

public interface IInventoryAdjustmentService
{
    Task<Result<PagedResponse<InventoryAdjustmentListDto>>> GetAllAsync(InventoryAdjustmentListFilter filter, CancellationToken ct = default);
    Task<Result<InventoryAdjustmentDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(InventoryAdjustmentCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, InventoryAdjustmentUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryAdjustmentPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryRegisterBalanceListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default);
}
