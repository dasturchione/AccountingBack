using Application.Features.InventoryMovements;
using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.WarehouseTransfers;

public interface IWarehouseTransferService
{
    Task<Result<PagedResponse<WarehouseTransferListDto>>> GetAllAsync(WarehouseTransferListFilter filter, CancellationToken ct = default);
    Task<Result<WarehouseTransferDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(WarehouseTransferCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, WarehouseTransferUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<List<WarehouseTransferPostingBatchDto>>> GetPostingBatchesAsync(long id, CancellationToken ct = default);
    Task<Result<List<InventoryMovementListDto>>> GetInventoryMovementsAsync(long id, CancellationToken ct = default);
}
