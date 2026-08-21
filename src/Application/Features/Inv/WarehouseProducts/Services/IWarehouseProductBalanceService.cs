using Application.Features.InventoryMovements;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.Inv.WarehouseProducts;

public interface IWarehouseProductBalanceService
{
    Task<Result> ApplyInventoryEntriesAsync(IReadOnlyCollection<InventoryMovementEntry> entries, CancellationToken ct = default);

    Task<Result> ApplySaleInventoryEntriesAsync(
        SaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default);

    Task<Result> ApplyRetailSaleInventoryEntriesAsync(
        RetailSaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default);

    Task<Result> ReverseSaleInventoryEntriesAsync(
        SaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default);

    Task<Result> ReverseRetailSaleInventoryEntriesAsync(
        RetailSaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default);

    Task<Result> ReserveAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default);

    Task<Result> ReserveAsync(
        int warehouseId,
        IReadOnlyCollection<WarehouseProductBalanceItem> items,
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct = default);

    Task<Result> ReleaseReservedAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default);

    Task<Result> ReleaseReservedAsync(
        int warehouseId,
        IReadOnlyCollection<WarehouseProductBalanceItem> items,
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct = default);
}
