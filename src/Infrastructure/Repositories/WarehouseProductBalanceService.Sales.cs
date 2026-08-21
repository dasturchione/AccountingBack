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

        var markingResult = await ConsumePhysicalMarkingsAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            GetSalePhysicalMarkingIds(sale),
            DocumentTypeIdConst.SALE,
            sale.Id,
            ct);
        if (!markingResult.IsSuccess)
            return markingResult;

        var productLines = sale.SaleDocProducts.Where(line => !line.Product.IsService).ToList();
        var productTables = productLines.SelectMany(line => line.SaleDocTables).ToList();
        if (productLines.Count > 0)
            await _saleDocProductCommand.UpdateAsync(productLines, ct);
        if (productTables.Count > 0)
            await _saleDocTableCommand.UpdateAsync(productTables, ct);

        return Result.Success();
    }

    public async Task<Result> ApplyRetailSaleInventoryEntriesAsync(
        RetailSaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default)
    {
        var inventoryResult = await ApplyInventoryEntriesAsync(entries, ct);
        if (!inventoryResult.IsSuccess)
            return inventoryResult;

        return await ConsumePhysicalMarkingsAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            GetRetailSalePhysicalMarkingIds(sale),
            DocumentTypeIdConst.RETAIL_SALE,
            sale.Id,
            ct);
    }

    public async Task<Result> ReverseSaleInventoryEntriesAsync(
        SaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default)
    {
        var inventoryResult = await ApplyInventoryEntriesAsync(entries, ct);
        if (!inventoryResult.IsSuccess)
            return inventoryResult;

        return await RestorePhysicalMarkingsAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            GetSalePhysicalMarkingIds(sale),
            DocumentTypeIdConst.SALE,
            sale.Id,
            ct);
    }

    public async Task<Result> ReverseRetailSaleInventoryEntriesAsync(
        RetailSaleDoc sale,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct = default)
    {
        var inventoryResult = await ApplyInventoryEntriesAsync(entries, ct);
        if (!inventoryResult.IsSuccess)
            return inventoryResult;

        return await RestorePhysicalMarkingsAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            GetRetailSalePhysicalMarkingIds(sale),
            DocumentTypeIdConst.RETAIL_SALE,
            sale.Id,
            ct);
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

            var allocationResult = await AllocateNonPieceTrackedProductAsync(
                sale, productLine, batches, availableQuantityByBatchId, ct);
            if (!allocationResult.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(allocationResult.Error);

            var allocation = allocationResult.Value;
            var persistedAllocations = await CreateSaleBatchAllocationsAsync(productLine, allocation.BatchAllocations, ct);
            if (!persistedAllocations.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(persistedAllocations.Error);

            foreach (var sourceLineId in new[] { productLine.Id })
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

        if (!SaleMarkingPolicy.IsOccurrenceCountAllowed(productLine.Quantity, productLine.SaleDocTables.Count))
        {
            return Task.FromResult(Result.Failure(
                WarehouseProductErrors.InvalidSaleAllocation(productLine.Id, _userContext.LanguageId)));
        }

        if (entriesBySourceLineId[productLine.Id].Count() != 1)
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

    private async Task<Result> ConsumePhysicalMarkingsAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyCollection<int> productTableIds,
        short documentTypeId,
        long documentId,
        CancellationToken ct)
    {
        if (productTableIds.Count == 0)
            return Result.Success();

        var usedByAnotherSale = await _saleDocTableQuery.AnyAsync(item =>
            productTableIds.Contains(item.ProductTableId) &&
            item.Owner.Owner.OrganizationId == organizationId &&
            item.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED &&
            (documentTypeId != DocumentTypeIdConst.SALE || item.Owner.OwnerId != documentId),
            ct);
        var usedByAnotherRetailSale = await _retailSaleDocTableQuery.AnyAsync(item =>
            productTableIds.Contains(item.ProductTableId) &&
            item.Owner.Owner.OrganizationId == organizationId &&
            item.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED &&
            (documentTypeId != DocumentTypeIdConst.RETAIL_SALE || item.Owner.OwnerId != documentId),
            ct);
        if (usedByAnotherSale || usedByAnotherRetailSale)
            return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                productTableIds.First(),
                ProductTableStatusIdConst.SOLD,
                _userContext.LanguageId));

        var organizationTableIds = (await _productTableQuery.GetAllAsync(
                _queryBuilder.For<ProductTable>()
                    .Where(table => productTableIds.Contains(table.Id) &&
                                    table.Product.OrganizationId == organizationId)
                    .As(table => table.Id)
                    .Build(),
                ct))
            .ToHashSet();
        if (organizationTableIds.Count != productTableIds.Count)
            return Result.Failure(WarehouseProductErrors.ProductTableNotFound(
                productTableIds.First(id => !organizationTableIds.Contains(id)),
                _userContext.LanguageId));

        var tables = await _warehouseProductTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductTable>()
                .Where(table => productTableIds.Contains(table.ProductTableId))
                .Build(),
            ct);
        foreach (var table in tables)
            await _warehouseProductTableCommand.ReloadAsync(table, ct);

        var byId = tables.ToDictionary(table => table.ProductTableId);
        foreach (var productTableId in productTableIds)
        {
            if (!byId.TryGetValue(productTableId, out var table) ||
                table.WarehouseId != warehouseId ||
                !SaleMarkingPolicy.IsAvailableForSale(table.StatusId))
            {
                return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                    productTableId,
                    byId.GetValueOrDefault(productTableId)?.StatusId ?? 0,
                    _userContext.LanguageId));
            }
        }

        foreach (var table in tables)
            table.StatusId = ProductTableStatusIdConst.SOLD;
        await _warehouseProductTableCommand.UpdateAsync(tables, ct);
        return Result.Success();
    }

    private async Task<Result> RestorePhysicalMarkingsAsync(
        int organizationId,
        int warehouseId,
        IReadOnlyCollection<int> productTableIds,
        short documentTypeId,
        long documentId,
        CancellationToken ct)
    {
        if (productTableIds.Count == 0)
            return Result.Success();

        var usedByAnotherSale = await _saleDocTableQuery.AnyAsync(item =>
            productTableIds.Contains(item.ProductTableId) &&
            item.Owner.Owner.OrganizationId == organizationId &&
            item.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED &&
            (documentTypeId != DocumentTypeIdConst.SALE || item.Owner.OwnerId != documentId),
            ct);
        var usedByAnotherRetailSale = await _retailSaleDocTableQuery.AnyAsync(item =>
            productTableIds.Contains(item.ProductTableId) &&
            item.Owner.Owner.OrganizationId == organizationId &&
            item.Owner.Owner.StatusId == DocumentStatusIdConst.POSTED &&
            (documentTypeId != DocumentTypeIdConst.RETAIL_SALE || item.Owner.OwnerId != documentId),
            ct);
        if (usedByAnotherSale || usedByAnotherRetailSale)
            return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                productTableIds.First(),
                ProductTableStatusIdConst.SOLD,
                _userContext.LanguageId));

        var tables = await _warehouseProductTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductTable>()
                .Where(table => productTableIds.Contains(table.ProductTableId) &&
                                table.ProductTable.Product.OrganizationId == organizationId)
                .Build(),
            ct);
        foreach (var table in tables)
            await _warehouseProductTableCommand.ReloadAsync(table, ct);

        var byId = tables.ToDictionary(table => table.ProductTableId);
        foreach (var productTableId in productTableIds)
        {
            if (!byId.TryGetValue(productTableId, out var table) ||
                table.WarehouseId != warehouseId ||
                !SaleMarkingPolicy.IsRestorableAfterSale(table.StatusId))
            {
                return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                    productTableId,
                    byId.GetValueOrDefault(productTableId)?.StatusId ?? 0,
                    _userContext.LanguageId));
            }
        }

        foreach (var table in tables)
            table.StatusId = ProductTableStatusIdConst.IN_STOCK;
        await _warehouseProductTableCommand.UpdateAsync(tables, ct);
        return Result.Success();
    }

    private static IReadOnlyList<int> GetSalePhysicalMarkingIds(SaleDoc sale) =>
        SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(
            sale.SaleDocProducts.Select(line => line.SaleDocTables.Select(item => item.ProductTableId)));

    private static IReadOnlyList<int> GetRetailSalePhysicalMarkingIds(RetailSaleDoc sale) =>
        SaleMarkingPolicy.GetDistinctPhysicalMarkingIds(
            sale.RetailSaleDocProducts.Select(line => line.RetailSaleDocTables.Select(item => item.ProductTableId)));

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
