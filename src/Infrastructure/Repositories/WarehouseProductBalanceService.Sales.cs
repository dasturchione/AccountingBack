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
        var allocationPlan = await BuildMarkingBatchAllocationPlanAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            DocumentTypeIdConst.RETAIL_SALE,
            sale.Id,
            entries,
            sale.RetailSaleDocProducts
                .Where(line => !line.Product.IsService)
                .SelectMany(line => line.RetailSaleDocTables.Select(item =>
                    new SaleMarkingOccurrence(line.Id, line.ProductId, item.ProductTableId)))
                .ToArray(),
            ct);
        if (!allocationPlan.IsSuccess)
            return Result.Failure(allocationPlan.Error);

        var inventoryResult = await ApplyInventoryEntriesAsync(entries, allocationPlan.Value.EntryAllocations, ct);
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

        foreach (var productLine in sale.SaleDocProducts
                     .Where(line => !line.Product.IsService)
                     .OrderBy(line => line.ProductId)
                     .ThenBy(line => line.Id))
        {
            var validation = await ValidateSaleProductAsync(sale, productLine, entriesBySourceLineId, ct);
            if (!validation.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(validation.Error);
        }

        var planResult = await BuildMarkingBatchAllocationPlanAsync(
            sale.OrganizationId,
            sale.WarehouseId,
            DocumentTypeIdConst.SALE,
            sale.Id,
            entries,
            sale.SaleDocProducts
                .Where(line => !line.Product.IsService)
                .SelectMany(line => line.SaleDocTables.Select(item =>
                    new SaleMarkingOccurrence(line.Id, line.ProductId, item.ProductTableId)))
                .ToArray(),
            ct);
        if (!planResult.IsSuccess)
            return Result.Failure<SaleInventoryAllocationPlan>(planResult.Error);

        var plan = planResult.Value;
        foreach (var productLine in sale.SaleDocProducts.Where(line => !line.Product.IsService))
        {
            var entry = entriesBySourceLineId[productLine.Id].Single();
            var persistedAllocations = await CreateSaleBatchAllocationsAsync(
                productLine,
                plan.EntryAllocations[entry],
                ct);
            if (!persistedAllocations.IsSuccess)
                return Result.Failure<SaleInventoryAllocationPlan>(persistedAllocations.Error);
        }

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

    private async Task<Result<SaleInventoryAllocationPlan>> BuildMarkingBatchAllocationPlanAsync(
        int organizationId,
        int warehouseId,
        short documentTypeId,
        long documentId,
        IReadOnlyCollection<InventoryMovementEntry> entries,
        IReadOnlyCollection<SaleMarkingOccurrence> markingOccurrences,
        CancellationToken ct)
    {
        if (entries.Any(entry =>
                entry.OrganizationId != organizationId ||
                entry.WarehouseId != warehouseId ||
                entry.DocumentTypeId != documentTypeId ||
                entry.DocumentId != documentId ||
                entry.OperationTypeId != OperationTypeIdConst.OUT ||
                !entry.SourceLineId.HasValue))
        {
            return Result.Failure<SaleInventoryAllocationPlan>(
                WarehouseProductErrors.InvalidSaleAllocation(documentId, _userContext.LanguageId));
        }

        var plan = new SaleInventoryAllocationPlan();
        foreach (var entry in entries)
            plan.EntryAllocations[entry] = Array.Empty<ProductBatchAllocation>();

        if (markingOccurrences.Count == 0)
            return Result.Success(plan);

        var productTableIds = markingOccurrences
            .Select(occurrence => occurrence.ProductTableId)
            .Distinct()
            .ToList();
        var productTables = await _productTableQuery.GetAllAsync(
            _queryBuilder.For<ProductTable>()
                .Where(table => productTableIds.Contains(table.Id) &&
                                table.Product.OrganizationId == organizationId)
                .As(table => new ProductTableMarkingIdentity(table.Id, table.ProductId, table.MarkingNumber))
                .Build(),
            ct);
        if (productTables.Count != productTableIds.Count)
        {
            var foundIds = productTables.Select(table => table.ProductTableId).ToHashSet();
            return Result.Failure<SaleInventoryAllocationPlan>(WarehouseProductErrors.ProductTableNotFound(
                productTableIds.First(id => !foundIds.Contains(id)),
                _userContext.LanguageId));
        }

        var selectedMarkings = SaleMarkingPolicy.SelectBatchMarkings(markingOccurrences, productTables);
        if (selectedMarkings.Count == 0)
            return Result.Success(plan);

        var entriesByLineId = entries
            .GroupBy(entry => entry.SourceLineId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());
        foreach (var selection in selectedMarkings)
        {
            if (!entriesByLineId.TryGetValue(selection.LineId, out var lineEntries) ||
                lineEntries.Count != 1 ||
                lineEntries[0].ProductId != selection.ProductId)
            {
                return Result.Failure<SaleInventoryAllocationPlan>(
                    WarehouseProductErrors.InvalidSaleAllocation(selection.LineId, _userContext.LanguageId));
            }
        }

        var selectedProductTableIds = selectedMarkings
            .Select(selection => selection.ProductTableId)
            .Distinct()
            .ToList();
        var batchLinks = await _warehouseProductBatchTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatchTable>()
                .Where(link => selectedProductTableIds.Contains(link.ProductTableId) &&
                               link.Batch.OrganizationId == organizationId &&
                               link.Batch.WarehouseId == warehouseId &&
                               link.Batch.ProductId == link.ProductTable.ProductId)
                .As(link => new MarkingBatchLink(
                    link.ProductTableId,
                    link.BatchId,
                    link.Batch.RemainingQuantity,
                    link.Batch.ReceivedDate))
                .Build(),
            ct);

        var remainingQuantityByBatchId = batchLinks
            .GroupBy(link => link.BatchId)
            .ToDictionary(group => group.Key, group => group.First().RemainingQuantity);
        var productTableIdsByLineAndBatch = new Dictionary<(long LineId, long BatchId), List<int>>();

        foreach (var selection in selectedMarkings)
        {
            var batchLink = batchLinks
                .Where(link => link.ProductTableId == selection.ProductTableId &&
                               remainingQuantityByBatchId.GetValueOrDefault(link.BatchId) >= 1m)
                .OrderBy(link => link.ReceivedDate)
                .ThenBy(link => link.BatchId)
                .FirstOrDefault();
            if (batchLink is null)
                continue;

            remainingQuantityByBatchId[batchLink.BatchId] -= 1m;
            var key = (selection.LineId, batchLink.BatchId);
            if (!productTableIdsByLineAndBatch.TryGetValue(key, out var allocatedProductTableIds))
            {
                allocatedProductTableIds = [];
                productTableIdsByLineAndBatch[key] = allocatedProductTableIds;
            }

            allocatedProductTableIds.Add(selection.ProductTableId);
        }

        foreach (var group in productTableIdsByLineAndBatch.GroupBy(pair => pair.Key.LineId))
        {
            var entry = entriesByLineId[group.Key].Single();
            plan.EntryAllocations[entry] = group
                .OrderBy(pair => pair.Key.BatchId)
                .Select(pair => new ProductBatchAllocation
                {
                    BatchId = pair.Key.BatchId,
                    Quantity = pair.Value.Count,
                    ProductTableIds = pair.Value.OrderBy(id => id).ToArray()
                })
                .ToArray();
        }

        return Result.Success(plan);
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
        var allocatedQuantity = allocations.Sum(allocation => allocation.Quantity);
        if (allocations.Any(allocation =>
                allocation.Quantity <= 0m ||
                allocation.Quantity != allocation.ProductTableIds.Distinct().Count()) ||
            allocatedQuantity > entry.Quantity)
        {
            return Result.Failure(WarehouseProductErrors.InvalidSaleAllocation(entry.SourceLineId ?? entry.DocumentId, _userContext.LanguageId));
        }

        if (allocations.Count == 0)
            return Result.Success();

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

    private sealed class SaleInventoryAllocationPlan
    {
        public Dictionary<InventoryMovementEntry, IReadOnlyList<ProductBatchAllocation>> EntryAllocations { get; } = new();
    }

    private sealed record MarkingBatchLink(
        int ProductTableId,
        long BatchId,
        decimal RemainingQuantity,
        DateTime ReceivedDate);
}
