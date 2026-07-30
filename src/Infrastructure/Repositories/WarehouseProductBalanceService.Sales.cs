using Application.Features.Inv.WarehouseProducts;
using Application.Features.InventoryMovements;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Infrastructure.Repositories;

public partial class WarehouseProductBalanceService
{
    public async Task<Result> ApplySaleInventoryEntriesAsync(
        SaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default)
    {
        var allocationPlan = await BuildSaleAllocationPlanAsync(sale, entries, ct);
        if (!allocationPlan.IsSuccess)
            return Result.Failure(allocationPlan.Error);

        var movementResult = await CreateWarehouseMovementsAsync(entries, allocationPlan.Value.EntryAllocations, ct);
        if (!movementResult.IsSuccess)
            return movementResult;

        var productLines = sale.SaleDocProducts.Where(line => !line.Product.IsService).ToList();
        var productTables = productLines.SelectMany(line => line.SaleDocTables).ToList();
        if (productLines.Count > 0)
            await _saleDocProductCommand.UpdateAsync(productLines, ct);
        if (productTables.Count > 0)
            await _saleDocTableCommand.UpdateAsync(productTables, ct);

        return Result.Success();
    }

    private async Task<Result> CreateWarehouseMovementsAsync(
        IReadOnlyCollection<InventoryMovementEntry> entries,
        IReadOnlyDictionary<InventoryMovementEntry, IReadOnlyList<ProductBatchAllocation>> allocations,
        CancellationToken ct) =>
        await ApplyInventoryEntriesAsync(entries, allocations, ct);

    private async Task<Result<SaleInventoryAllocationPlan>> BuildSaleAllocationPlanAsync(
        SaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct)
    {
        if (entries.Any(entry => entry.DocumentTypeId != DocumentTypeIdConst.SALE || entry.DocumentId != sale.Id))
            return Result.Failure<SaleInventoryAllocationPlan>(WarehouseProductErrors.InvalidSaleAllocation(sale.Id, _userContext.LanguageId));

        var entriesBySourceLineId = entries
            .Where(entry => entry.SourceLineId.HasValue)
            .ToLookup(entry => entry.SourceLineId!.Value);
        var lockedBatchesByProductId = new Dictionary<int, List<WarehouseProductBatch>>();
        var availableQuantityByBatchId = new Dictionary<long, decimal>();
        var plan = new SaleInventoryAllocationPlan();

        foreach (var productLine in sale.SaleDocProducts
                     .Where(line => !line.Product.IsService)
                     .OrderBy(line => line.ProductId)
                     .ThenBy(line => line.Id))
        {
            var validation = await ValidateSaleProductAsync(sale, productLine, entriesBySourceLineId, ct);
            if (!validation.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(validation.Error);

            if (!lockedBatchesByProductId.TryGetValue(productLine.ProductId, out var batches))
            {
                batches = await LockAvailableBatchesAsync(sale.OrganizationId, sale.WarehouseId, productLine.ProductId, ct);
                lockedBatchesByProductId[productLine.ProductId] = batches;
                foreach (var batch in batches)
                    availableQuantityByBatchId.TryAdd(batch.Id, batch.RemainingQuantity);
            }

            Result<SaleProductAllocation> allocationResult = productLine.Product.IsPieceTracked
                ? await AllocatePieceTrackedProductAsync(sale, productLine, batches, availableQuantityByBatchId, ct)
                : await AllocateNonPieceTrackedProductAsync(sale, productLine, batches, availableQuantityByBatchId, ct);
            if (!allocationResult.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(allocationResult.Error);

            var allocation = allocationResult.Value;
            var persistedAllocations = await CreateSaleBatchAllocationsAsync(productLine, allocation.BatchAllocations, ct);
            if (!persistedAllocations.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(persistedAllocations.Error);

            var allocationSourceLineIds = productLine.Product.IsPieceTracked
                ? productLine.SaleDocTables.Select(table => table.Id)
                : new[] { productLine.Id };
            foreach (var sourceLineId in allocationSourceLineIds)
            {
                if (!allocation.EntryAllocations.TryGetValue(sourceLineId, out var entryAllocations))
                    return Result.Failure<SaleInventoryAllocationPlan>(WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId));

                foreach (var entry in entriesBySourceLineId[sourceLineId])
                    plan.EntryAllocations[entry] = entryAllocations;
            }
        }

        if (plan.EntryAllocations.Count != entries.Count)
            return Result.Failure<SaleInventoryAllocationPlan>(WarehouseProductErrors.InvalidSaleAllocation(sale.Id, _userContext.LanguageId));

        return Result.Success(plan);
    }

    private Task<Result> ValidateSaleProductAsync(
        SaleDoc sale,
        SaleDocProduct productLine,
        ILookup<long, InventoryMovementEntry> entriesBySourceLineId,
        CancellationToken ct)
    {
        if (productLine.Quantity <= 0m ||
            (productLine.Product.IsPieceTracked && productLine.Quantity != decimal.Truncate(productLine.Quantity)))
        {
            return Task.FromResult(Result.Failure(
                WarehouseProductErrors.InvalidQuantity(productLine.ProductId, productLine.Quantity, _userContext.LanguageId)));
        }

        if (!productLine.Product.IsPieceTracked)
        {
            if (productLine.SaleDocTables.Count > 0 || entriesBySourceLineId[productLine.Id].Count() != 1)
            {
                return Task.FromResult(Result.Failure(
                    WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
            }

            var entry = entriesBySourceLineId[productLine.Id].Single();
            return Task.FromResult(entry.ProductId == productLine.ProductId &&
                                   !entry.ProductTableId.HasValue &&
                                   entry.Quantity == productLine.Quantity
                ? Result.Success()
                : Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
        }

        if (productLine.SaleDocTables.Count != (int)productLine.Quantity ||
            productLine.SaleDocTables.Select(table => table.ProductTableId).Distinct().Count() != productLine.SaleDocTables.Count)
        {
            return Task.FromResult(Result.Failure(
                WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
        }

        var saleTableIds = productLine.SaleDocTables.Select(table => table.Id).ToHashSet();
        var entries = productLine.SaleDocTables.SelectMany(table => entriesBySourceLineId[table.Id]).ToList();
        return Task.FromResult(entries.Count == productLine.SaleDocTables.Count &&
                               entries.All(entry => entry.ProductId == productLine.ProductId &&
                                                    entry.ProductTableId.HasValue &&
                                                    entry.Quantity == 1m &&
                                                    entry.SourceLineId.HasValue &&
                                                    saleTableIds.Contains(entry.SourceLineId.Value))
            ? Result.Success()
            : Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
    }

    private async Task<Result<SaleProductAllocation>> AllocateNonPieceTrackedProductAsync(
        SaleDoc sale,
        SaleDocProduct productLine,
        IReadOnlyCollection<WarehouseProductBatch> batches,
        IDictionary<long, decimal> availableQuantityByBatchId,
        CancellationToken ct)
    {
        var batchesById = batches.ToDictionary(batch => batch.Id);
        var selectedValidation = await ValidateSelectedBatchesAsync(
            sale,
            productLine,
            productLine.SaleDocProductBatches,
            batchesById,
            availableQuantityByBatchId,
            ct);
        if (!selectedValidation.IsSuccess)
            return Result.Failure<SaleProductAllocation>(selectedValidation.Error);

        var allocatedQuantityByBatchId = new Dictionary<long, decimal>();
        foreach (var selectedBatch in productLine.SaleDocProductBatches)
        {
            AddAllocatedQuantity(allocatedQuantityByBatchId, selectedBatch.WarehouseProductBatchId, selectedBatch.Quantity);
            availableQuantityByBatchId[selectedBatch.WarehouseProductBatchId] -= selectedBatch.Quantity;
        }

        var quantityToAllocate = productLine.Quantity - productLine.SaleDocProductBatches.Sum(item => item.Quantity);
        foreach (var batch in batches.OrderBy(batch => batch.ReceivedDate).ThenBy(batch => batch.Id))
        {
            if (quantityToAllocate <= 0m)
                break;

            var availableQuantity = GetAvailableQuantity(availableQuantityByBatchId, batch.Id);
            if (availableQuantity <= 0m)
                continue;

            var quantity = Math.Min(quantityToAllocate, availableQuantity);
            AddAllocatedQuantity(allocatedQuantityByBatchId, batch.Id, quantity);
            availableQuantityByBatchId[batch.Id] -= quantity;
            quantityToAllocate -= quantity;
        }

        if (quantityToAllocate > 0m)
        {
            var availableQuantity = batches.Sum(batch => GetAvailableQuantity(availableQuantityByBatchId, batch.Id)) +
                                    allocatedQuantityByBatchId.Values.Sum();
            return Result.Failure<SaleProductAllocation>(WarehouseProductErrors.NotEnoughQuantity(
                sale.WarehouseId,
                productLine.ProductId,
                productLine.Quantity,
                availableQuantity,
                _userContext.LanguageId));
        }

        var allocations = ToBatchAllocations(allocatedQuantityByBatchId);

        return Result.Success(new SaleProductAllocation(
            allocations,
            new Dictionary<long, IReadOnlyList<ProductBatchAllocation>>
            {
                [productLine.Id] = allocations
            }));
    }

    private async Task<Result<SaleProductAllocation>> AllocatePieceTrackedProductAsync(
        SaleDoc sale,
        SaleDocProduct productLine,
        IReadOnlyCollection<WarehouseProductBatch> batches,
        IDictionary<long, decimal> availableQuantityByBatchId,
        CancellationToken ct)
    {
        var selectedProductTableIds = productLine.SaleDocTables
            .Select(table => table.ProductTableId)
            .ToList();
        var lockedWarehouseProductTables = await LockWarehouseProductTablesAsync(selectedProductTableIds, ct);
        var productTables = (await _productTableQuery.GetAllAsync(
                _queryBuilder.For<ProductTable>()
                    .Where(table => selectedProductTableIds.Contains(table.Id))
                    .As(table => new SaleProductTableSnapshot(table.Id, table.ProductId, table.Product.OrganizationId))
                    .Build(),
                ct))
            .ToDictionary(table => table.Id);
        var productTableValidation = await ValidateSelectedProductTablesAsync(
            sale,
            productLine,
            productTables,
            lockedWarehouseProductTables,
            ct);
        if (!productTableValidation.IsSuccess)
            return Result.Failure<SaleProductAllocation>(productTableValidation.Error);

        var batchIds = batches.Select(batch => batch.Id).ToList();
        var batchLinks = await _warehouseProductBatchTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatchTable>()
                .Where(link => batchIds.Contains(link.BatchId) && selectedProductTableIds.Contains(link.ProductTableId))
                .Build(),
            ct);
        var batchesById = batches.ToDictionary(batch => batch.Id);
        if (productLine.SaleDocProductBatches.Count > 0)
        {
            var selectedValidation = await ValidateSelectedBatchesAsync(
                sale,
                productLine,
                productLine.SaleDocProductBatches,
                batchesById,
                availableQuantityByBatchId,
                ct);
            if (!selectedValidation.IsSuccess)
                return Result.Failure<SaleProductAllocation>(selectedValidation.Error);
        }

        var productTableToBatch = new Dictionary<int, WarehouseProductBatch>();
        var allocatedTableIdsByBatchId = new Dictionary<long, List<int>>();

        foreach (var saleTable in productLine.SaleDocTables.OrderBy(table => table.ProductTableId))
        {
            var batch = batchLinks
                .Where(link => link.ProductTableId == saleTable.ProductTableId &&
                               batchesById.ContainsKey(link.BatchId) &&
                               GetAvailableQuantity(availableQuantityByBatchId, link.BatchId) >= 1m)
                .OrderBy(link => batchesById[link.BatchId].ReceivedDate)
                .ThenBy(link => link.BatchId)
                .Select(link => batchesById[link.BatchId])
                .FirstOrDefault();
            if (batch is null)
            {
                return Result.Failure<SaleProductAllocation>(WarehouseProductErrors.SelectedBatchUnavailable(
                    0,
                    sale.WarehouseId,
                    productLine.ProductId,
                    _userContext.LanguageId));
            }

            availableQuantityByBatchId[batch.Id] -= 1m;
            productTableToBatch[saleTable.ProductTableId] = batch;
            if (!allocatedTableIdsByBatchId.TryGetValue(batch.Id, out var productTableIds))
            {
                productTableIds = new List<int>();
                allocatedTableIdsByBatchId[batch.Id] = productTableIds;
            }

            productTableIds.Add(saleTable.ProductTableId);
        }

        var allocations = allocatedTableIdsByBatchId
            .OrderBy(pair => batchesById[pair.Key].ReceivedDate)
            .ThenBy(pair => pair.Key)
            .Select(pair => new ProductBatchAllocation
            {
                BatchId = pair.Key,
                Quantity = pair.Value.Count,
                ProductTableIds = pair.Value.OrderBy(id => id).ToList()
            })
            .ToList();

        if (productLine.SaleDocProductBatches.Count > 0)
        {
            var selectedByBatchId = productLine.SaleDocProductBatches.ToDictionary(item => item.WarehouseProductBatchId, item => item.Quantity);
            if (selectedByBatchId.Count != allocations.Count ||
                allocations.Any(allocation => selectedByBatchId.GetValueOrDefault(allocation.BatchId) != allocation.Quantity))
            {
                return Result.Failure<SaleProductAllocation>(WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId));
            }
        }

        var entryAllocations = productLine.SaleDocTables.ToDictionary(
            saleTable => saleTable.Id,
            saleTable => (IReadOnlyList<ProductBatchAllocation>)new List<ProductBatchAllocation>
            {
                new()
                {
                    BatchId = productTableToBatch[saleTable.ProductTableId].Id,
                    Quantity = 1m,
                    ProductTableIds = new[] { saleTable.ProductTableId }
                }
            });

        return Result.Success(new SaleProductAllocation(allocations, entryAllocations));
    }

    private Task<Result> ValidateSelectedBatchesAsync(
        SaleDoc sale,
        SaleDocProduct productLine,
        IEnumerable<SaleDocProductBatch> selectedBatches,
        IReadOnlyDictionary<long, WarehouseProductBatch> batchesById,
        IDictionary<long, decimal> availableQuantityByBatchId,
        CancellationToken ct)
    {
        if (selectedBatches.GroupBy(item => item.WarehouseProductBatchId).Any(group => group.Count() > 1) ||
            selectedBatches.Any(item => item.Quantity <= 0m) ||
            selectedBatches.Sum(item => item.Quantity) > productLine.Quantity)
        {
            return Task.FromResult(Result.Failure(
                WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
        }

        foreach (var selectedBatch in selectedBatches)
        {
            if (!batchesById.TryGetValue(selectedBatch.WarehouseProductBatchId, out var batch) ||
                batch.OrganizationId != sale.OrganizationId ||
                batch.WarehouseId != sale.WarehouseId ||
                batch.ProductId != productLine.ProductId)
            {
                return Task.FromResult(Result.Failure(WarehouseProductErrors.SelectedBatchUnavailable(
                    selectedBatch.WarehouseProductBatchId,
                    sale.WarehouseId,
                    productLine.ProductId,
                    _userContext.LanguageId)));
            }

            var availableQuantity = GetAvailableQuantity(availableQuantityByBatchId, batch.Id);
            if (availableQuantity < selectedBatch.Quantity)
            {
                return Task.FromResult(Result.Failure(WarehouseProductErrors.SelectedBatchNotEnoughQuantity(
                    batch.Id,
                    selectedBatch.Quantity,
                    availableQuantity,
                    _userContext.LanguageId)));
            }
        }

        return Task.FromResult(Result.Success());
    }

    private Task<Result> ValidateSelectedProductTablesAsync(
        SaleDoc sale,
        SaleDocProduct productLine,
        IReadOnlyDictionary<int, SaleProductTableSnapshot> productTables,
        IReadOnlyDictionary<int, WarehouseProductTable> warehouseProductTables,
        CancellationToken ct)
    {
        foreach (var saleTable in productLine.SaleDocTables)
        {
            if (!productTables.TryGetValue(saleTable.ProductTableId, out var productTable) ||
                productTable.ProductId != productLine.ProductId ||
                productTable.OrganizationId != sale.OrganizationId ||
                !warehouseProductTables.TryGetValue(saleTable.ProductTableId, out var warehouseProductTable) ||
                warehouseProductTable.WarehouseId != sale.WarehouseId ||
                warehouseProductTable.StatusId is not (ProductTableStatusIdConst.IN_STOCK or ProductTableStatusIdConst.RESERVED))
            {
                return Task.FromResult(Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                    saleTable.ProductTableId,
                    warehouseProductTables.GetValueOrDefault(saleTable.ProductTableId)?.StatusId ?? 0,
                    _userContext.LanguageId)));
            }
        }

        return Task.FromResult(Result.Success());
    }

    private async Task<Result> CreateSaleBatchAllocationsAsync(
        SaleDocProduct productLine,
        IReadOnlyCollection<ProductBatchAllocation> allocations,
        CancellationToken ct)
    {
        var existingByBatchId = productLine.SaleDocProductBatches
            .GroupBy(item => item.WarehouseProductBatchId)
            .ToDictionary(group => group.Key, group => group.ToList());
        if (existingByBatchId.Any(pair => pair.Value.Count > 1))
            return Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId));

        var allocationByBatchId = allocations.ToDictionary(item => item.BatchId);
        foreach (var existing in existingByBatchId.Values.SelectMany(items => items).ToList())
        {
            if (!allocationByBatchId.TryGetValue(existing.WarehouseProductBatchId, out var allocation))
            {
                await _saleDocProductBatchCommand.DeleteAsync(existing, ct);
                continue;
            }

            existing.Quantity = allocation.Quantity;
            await _saleDocProductBatchCommand.UpdateAsync(existing, ct);
        }

        var created = allocationByBatchId
            .Where(pair => !existingByBatchId.ContainsKey(pair.Key))
            .Select(pair => new SaleDocProductBatch
            {
                SaleDocProductId = productLine.Id,
                WarehouseProductBatchId = pair.Key,
                Quantity = pair.Value.Quantity
            })
            .ToList();
        if (created.Count > 0)
            await _saleDocProductBatchCommand.CreateAsync(created, ct);

        return Result.Success();
    }

    private async Task<Result> ApplySaleIssueMovementAsync(
        InventoryMovementEntry entry,
        WarehouseProductMovement issueMovement,
        IReadOnlyList<ProductBatchAllocation> allocations,
        CancellationToken ct)
    {
        if (allocations.Count == 0 || allocations.Sum(allocation => allocation.Quantity) != entry.Quantity)
            return Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(entry.SourceLineId ?? entry.DocumentId, _userContext.LanguageId));

        var batchIds = allocations.Select(allocation => allocation.BatchId).Distinct().ToList();
        var batchesById = (await _warehouseProductBatchQuery.GetAllAsync(
                _queryBuilder.For<WarehouseProductBatch>()
                    .Where(batch => batchIds.Contains(batch.Id))
                    .Build(),
                ct))
            .ToDictionary(batch => batch.Id);
        if (batchesById.Count != batchIds.Count)
            return Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(entry.SourceLineId ?? entry.DocumentId, _userContext.LanguageId));

        foreach (var allocation in allocations)
        {
            var batch = batchesById[allocation.BatchId];
            if (batch.OrganizationId != entry.OrganizationId ||
                batch.WarehouseId != entry.WarehouseId ||
                batch.ProductId != entry.ProductId)
            {
                return Result.Failure(WarehouseProductErrors.SelectedBatchUnavailable(
                    batch.Id,
                    entry.WarehouseId,
                    entry.ProductId,
                    _userContext.LanguageId));
            }

            if (batch.RemainingQuantity < allocation.Quantity)
            {
                return Result.Failure(WarehouseProductErrors.SelectedBatchNotEnoughQuantity(
                    batch.Id,
                    allocation.Quantity,
                    batch.RemainingQuantity,
                    _userContext.LanguageId));
            }

            var unitCost = batch.UnitCost ?? 0m;
            batch.RemainingQuantity -= allocation.Quantity;
            await _warehouseProductBatchCommand.UpdateAsync(batch, ct);
            await AddBatchAllocationAsync(issueMovement, batch, allocation.Quantity, unitCost, ct);
        }

        return Result.Success();
    }

    private async Task<List<WarehouseProductBatch>> LockAvailableBatchesAsync(
        int organizationId,
        int warehouseId,
        int productId,
        CancellationToken ct)
    {
        var batches = await _warehouseProductBatchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(batch => batch.OrganizationId == organizationId &&
                                batch.WarehouseId == warehouseId &&
                                batch.ProductId == productId &&
                                batch.RemainingQuantity > 0m)
                .Build(),
            ct);
        return batches.OrderBy(batch => batch.ReceivedDate).ThenBy(batch => batch.Id).ToList();
    }

    private async Task<Dictionary<int, WarehouseProductTable>> LockWarehouseProductTablesAsync(
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct)
    {
        var ids = productTableIds.Distinct().ToList();
        var tables = await _warehouseProductTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductTable>()
                .Where(table => ids.Contains(table.ProductTableId))
                .Build(),
            ct);
        return tables.ToDictionary(table => table.ProductTableId);
    }

    private static decimal GetAvailableQuantity(IDictionary<long, decimal> quantities, long batchId) =>
        quantities.TryGetValue(batchId, out var quantity) ? quantity : 0m;

    private static void AddAllocatedQuantity(IDictionary<long, decimal> quantities, long batchId, decimal quantity) =>
        quantities[batchId] = GetAvailableQuantity(quantities, batchId) + quantity;

    private static List<ProductBatchAllocation> ToBatchAllocations(IReadOnlyDictionary<long, decimal> quantities) =>
        quantities
            .Where(pair => pair.Value > 0m)
            .Select(pair => new ProductBatchAllocation { BatchId = pair.Key, Quantity = pair.Value })
            .ToList();

    private sealed record SaleProductTableSnapshot(int Id, int ProductId, int OrganizationId);

    private sealed class SaleInventoryAllocationPlan
    {
        public Dictionary<InventoryMovementEntry, IReadOnlyList<ProductBatchAllocation>> EntryAllocations { get; } = new();
    }

    private sealed class SaleProductAllocation
    {
        public SaleProductAllocation(
            IReadOnlyList<ProductBatchAllocation> batchAllocations,
            IReadOnlyDictionary<long, IReadOnlyList<ProductBatchAllocation>> entryAllocations)
        {
            BatchAllocations = batchAllocations;
            EntryAllocations = entryAllocations;
        }

        public IReadOnlyList<ProductBatchAllocation> BatchAllocations { get; }
        public IReadOnlyDictionary<long, IReadOnlyList<ProductBatchAllocation>> EntryAllocations { get; }
    }
}
