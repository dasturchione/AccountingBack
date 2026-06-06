using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryRegisterBalanceService
{
    Task<Result<PagedResponse<InventoryRegisterBalanceListDto>>> GetAllAsync(InventoryRegisterBalanceListFilter filter, CancellationToken ct = default);
    Task<Result<InventoryRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(InventoryRegisterBalanceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, InventoryRegisterBalanceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
