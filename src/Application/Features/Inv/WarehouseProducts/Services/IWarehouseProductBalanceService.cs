using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Inv.WarehouseProducts;

public interface IWarehouseProductBalanceService
{
    Task<Result> ApplyInventoryEntriesAsync(IReadOnlyCollection<RegisterBalance> entries, CancellationToken ct = default);

    Task<Result> ReserveAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default);

    Task<Result> ReleaseReservedAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default);
}
