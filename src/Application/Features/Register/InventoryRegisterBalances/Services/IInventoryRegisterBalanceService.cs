using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public interface IInventoryRegisterBalanceService
{
    Task<Result<PagedResponse<InventoryRegisterBalanceListDto>>> GetAllAsync(InventoryRegisterBalanceListFilter filter, CancellationToken ct = default);
    Task<Result<InventoryRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default);
}
