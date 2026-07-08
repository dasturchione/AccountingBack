using Application.Abstractions.Authentication;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Infrastructure.Repositories;

public class WarehouseProductBalanceService : IWarehouseProductBalanceService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;

    public WarehouseProductBalanceService(AppDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<Result> ApplyInventoryEntriesAsync(IReadOnlyCollection<RegisterBalance> entries, CancellationToken ct = default)
    {
        if (entries.Count == 0)
            return Result.Success();

        var changes = new List<WarehouseProductBalanceChange>();

        foreach (var entry in entries)
        {
            if (entry.Quantity < 0)
                return Result.Failure(WarehouseProductErrors.InvalidQuantity(entry.ProductId, entry.Quantity, _userContext.LanguageId));

            var quantityDelta = entry.OperationTypeId switch
            {
                OperationTypeIdConst.IN => entry.Quantity,
                OperationTypeIdConst.OUT => -entry.Quantity,
                _ => (decimal?)null
            };

            if (!quantityDelta.HasValue)
                return Result.Failure(WarehouseProductErrors.UnsupportedOperation(entry.OperationTypeId, _userContext.LanguageId));

            changes.Add(new WarehouseProductBalanceChange(
                entry.WarehouseId,
                entry.ProductId,
                UnitId: null,
                QuantityDelta: quantityDelta.Value,
                ReservedQuantityDelta: 0m,
                BlockedQuantityDelta: 0m));
        }

        return await ApplyChangesAsync(changes, initializeMissingFromProductTables: false, ct);
    }

    public Task<Result> ReserveAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default) =>
        ApplyReservedChangeAsync(warehouseId, items, sign: 1m, ct);

    public Task<Result> ReleaseReservedAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default) =>
        ApplyReservedChangeAsync(warehouseId, items, sign: -1m, ct);

    private Task<Result> ApplyReservedChangeAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, decimal sign, CancellationToken ct)
    {
        var invalidItem = items.FirstOrDefault(x => x.Quantity < 0m);
        if (invalidItem is not null)
            return Task.FromResult(Result.Failure(WarehouseProductErrors.InvalidQuantity(invalidItem.ProductId, invalidItem.Quantity, _userContext.LanguageId)));

        var changes = items.Select(x => new WarehouseProductBalanceChange(
                warehouseId,
                x.ProductId,
                x.UnitId,
                QuantityDelta: 0m,
                ReservedQuantityDelta: x.Quantity * sign,
                BlockedQuantityDelta: 0m))
            .ToList();

        return ApplyChangesAsync(changes, initializeMissingFromProductTables: true, ct);
    }

    private async Task<Result> ApplyChangesAsync(List<WarehouseProductBalanceChange> changes, bool initializeMissingFromProductTables, CancellationToken ct)
    {
        if (changes.Count == 0)
            return Result.Success();

        var normalized = changes
            .Where(x => x.QuantityDelta != 0m || x.ReservedQuantityDelta != 0m || x.BlockedQuantityDelta != 0m)
            .GroupBy(x => new { x.WarehouseId, x.ProductId })
            .Select(g => new WarehouseProductBalanceChange(
                g.Key.WarehouseId,
                g.Key.ProductId,
                g.Select(x => x.UnitId).FirstOrDefault(x => x.HasValue),
                g.Sum(x => x.QuantityDelta),
                g.Sum(x => x.ReservedQuantityDelta),
                g.Sum(x => x.BlockedQuantityDelta)))
            .ToList();

        if (normalized.Count == 0)
            return Result.Success();

        var productIds = normalized.Select(x => x.ProductId).Distinct().ToList();
        var productUnitIds = await _context.Set<Product>()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.UnitId })
            .ToDictionaryAsync(x => x.Id, x => x.UnitId, ct);

        foreach (var productId in productIds)
        {
            if (!productUnitIds.ContainsKey(productId))
                return Result.Failure(WarehouseProductErrors.ProductNotFound(productId, _userContext.LanguageId));
        }

        var warehouseIds = normalized.Select(x => x.WarehouseId).Distinct().ToList();
        var existing = await _context.Set<WarehouseProduct>()
            .Where(x => warehouseIds.Contains(x.WarehouseId) && productIds.Contains(x.ProductId))
            .ToListAsync(ct);

        var existingByKey = existing.ToDictionary(x => (x.WarehouseId, x.ProductId));
        var initialBalances = initializeMissingFromProductTables
            ? await GetInitialBalancesFromProductTablesAsync(normalized
                    .Where(x => !existingByKey.ContainsKey((x.WarehouseId, x.ProductId)))
                    .Select(x => (x.WarehouseId, x.ProductId))
                    .Distinct()
                    .ToList(),
                ct)
            : new Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>();

        var now = DateTime.Now;

        foreach (var change in normalized)
        {
            var key = (change.WarehouseId, change.ProductId);

            if (!existingByKey.TryGetValue(key, out var warehouseProduct))
            {
                initialBalances.TryGetValue(key, out var initialBalance);
                warehouseProduct = new WarehouseProduct
                {
                    WarehouseId = change.WarehouseId,
                    ProductId = change.ProductId,
                    UnitId = change.UnitId ?? productUnitIds[change.ProductId],
                    Quantity = initialBalance?.Quantity ?? 0m,
                    ReservedQuantity = initialBalance?.ReservedQuantity ?? 0m,
                    BlockedQuantity = initialBalance?.BlockedQuantity ?? 0m,
                    MinQuantity = 0m,
                    CreatedAt = now
                };

                await _context.Set<WarehouseProduct>().AddAsync(warehouseProduct, ct);
                existingByKey[key] = warehouseProduct;
            }

            var validation = ValidateChange(warehouseProduct, change);
            if (!validation.IsSuccess)
                return validation;

            warehouseProduct.Quantity += change.QuantityDelta;
            warehouseProduct.ReservedQuantity += change.ReservedQuantityDelta;
            warehouseProduct.BlockedQuantity += change.BlockedQuantityDelta;
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Result ValidateChange(WarehouseProduct warehouseProduct, WarehouseProductBalanceChange change)
    {
        if (change.QuantityDelta < 0m && warehouseProduct.Quantity < Math.Abs(change.QuantityDelta))
        {
            return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.QuantityDelta),
                warehouseProduct.Quantity,
                _userContext.LanguageId));
        }

        if (change.ReservedQuantityDelta < 0m && warehouseProduct.ReservedQuantity < Math.Abs(change.ReservedQuantityDelta))
        {
            return Result.Failure(WarehouseProductErrors.NotEnoughReserved(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.ReservedQuantityDelta),
                warehouseProduct.ReservedQuantity,
                _userContext.LanguageId));
        }

        if (change.BlockedQuantityDelta < 0m && warehouseProduct.BlockedQuantity < Math.Abs(change.BlockedQuantityDelta))
        {
            return Result.Failure(WarehouseProductErrors.NotEnoughBlocked(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.BlockedQuantityDelta),
                warehouseProduct.BlockedQuantity,
                _userContext.LanguageId));
        }

        var nextQuantity = warehouseProduct.Quantity + change.QuantityDelta;
        var nextReservedQuantity = warehouseProduct.ReservedQuantity + change.ReservedQuantityDelta;
        var nextBlockedQuantity = warehouseProduct.BlockedQuantity + change.BlockedQuantityDelta;
        var nextAvailableQuantity = nextQuantity - nextReservedQuantity - nextBlockedQuantity;

        if (nextAvailableQuantity < 0m)
        {
            var requested = Math.Max(-change.QuantityDelta, 0m) +
                            Math.Max(change.ReservedQuantityDelta, 0m) +
                            Math.Max(change.BlockedQuantityDelta, 0m);
            return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                requested,
                warehouseProduct.Quantity - warehouseProduct.ReservedQuantity - warehouseProduct.BlockedQuantity,
                _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>> GetInitialBalancesFromProductTablesAsync(
        IReadOnlyCollection<(int WarehouseId, int ProductId)> keys,
        CancellationToken ct)
    {
        if (keys.Count == 0)
            return new Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>();

        var warehouseIds = keys.Select(x => x.WarehouseId).Distinct().ToList();
        var productIds = keys.Select(x => x.ProductId).Distinct().ToList();

        var rows = await _context.Set<ProductTable>()
            .Where(x => x.CurrentWarehouseId.HasValue &&
                        warehouseIds.Contains(x.CurrentWarehouseId.Value) &&
                        productIds.Contains(x.ProductId) &&
                        x.StateId == StateIdConst.ACTIVE &&
                        (x.StatusId == ProductTableStatusIdConst.IN_STOCK ||
                         x.StatusId == ProductTableStatusIdConst.RESERVED ||
                         x.StatusId == ProductTableStatusIdConst.BLOCKED))
            .GroupBy(x => new { WarehouseId = x.CurrentWarehouseId!.Value, x.ProductId, x.StatusId })
            .Select(x => new
            {
                x.Key.WarehouseId,
                x.Key.ProductId,
                x.Key.StatusId,
                Quantity = x.Count()
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => (x.WarehouseId, x.ProductId))
            .ToDictionary(
                x => x.Key,
                x => new WarehouseProductInitialBalance(
                    x.Sum(r => (decimal)r.Quantity),
                    x.Where(r => r.StatusId == ProductTableStatusIdConst.RESERVED).Sum(r => (decimal)r.Quantity),
                    x.Where(r => r.StatusId == ProductTableStatusIdConst.BLOCKED).Sum(r => (decimal)r.Quantity)));
    }

    private sealed record WarehouseProductBalanceChange(
        int WarehouseId,
        int ProductId,
        short? UnitId,
        decimal QuantityDelta,
        decimal ReservedQuantityDelta,
        decimal BlockedQuantityDelta);

    private sealed record WarehouseProductInitialBalance(decimal Quantity, decimal ReservedQuantity, decimal BlockedQuantity);
}
