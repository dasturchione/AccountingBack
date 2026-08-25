using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.InventoryMovements;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Infrastructure.Repositories;

public partial class WarehouseProductBalanceService : IWarehouseProductBalanceService
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Product> _productQuery;
    private readonly IQueryRepository<ProductTable> _productTableQuery;
    private readonly IQueryRepository<WarehouseProduct> _warehouseProductQuery;
    private readonly IQueryRepository<WarehouseProductTable> _warehouseProductTableQuery;
    private readonly IQueryRepository<WarehouseProductMovement> _warehouseProductMovementQuery;
    private readonly IQueryRepository<WarehouseProductBatch> _warehouseProductBatchQuery;
    private readonly IQueryRepository<WarehouseProductBatchTable> _warehouseProductBatchTableQuery;
    private readonly IQueryRepository<WarehouseProductBatchAllocation> _warehouseProductBatchAllocationQuery;
    private readonly IQueryRepository<OrganizationConfig> _organizationConfigQuery;
    private readonly IQueryRepository<PurchaseDoc> _purchaseDocQuery;
    private readonly IQueryRepository<WarehouseTransferDoc> _warehouseTransferDocQuery;
    private readonly IQueryRepository<InventoryAdjustmentDoc> _inventoryAdjustmentDocQuery;
    private readonly IQueryRepository<OpeningInventory> _openingInventoryQuery;
    private readonly IQueryRepository<SaleDocTable> _saleDocTableQuery;
    private readonly IQueryRepository<RetailSaleDocTable> _retailSaleDocTableQuery;
    private readonly ICommandRepository<WarehouseProduct> _warehouseProductCommand;
    private readonly ICommandRepository<WarehouseProductTable> _warehouseProductTableCommand;
    private readonly ICommandRepository<WarehouseProductMovement> _warehouseProductMovementCommand;
    private readonly ICommandRepository<WarehouseProductBatch> _warehouseProductBatchCommand;
    private readonly ICommandRepository<WarehouseProductBatchTable> _warehouseProductBatchTableCommand;
    private readonly ICommandRepository<WarehouseProductBatchAllocation> _warehouseProductBatchAllocationCommand;
    private readonly ICommandRepository<SaleDocProduct> _saleDocProductCommand;
    private readonly ICommandRepository<SaleDocTable> _saleDocTableCommand;
    private readonly ICommandRepository<SaleDocProductBatch> _saleDocProductBatchCommand;

    public WarehouseProductBalanceService(
        IQueryBuilder queryBuilder,
        IUserContext userContext,
        IQueryRepository<Product> productQuery,
        IQueryRepository<ProductTable> productTableQuery,
        IQueryRepository<WarehouseProduct> warehouseProductQuery,
        IQueryRepository<WarehouseProductTable> warehouseProductTableQuery,
        IQueryRepository<WarehouseProductMovement> warehouseProductMovementQuery,
        IQueryRepository<WarehouseProductBatch> warehouseProductBatchQuery,
        IQueryRepository<WarehouseProductBatchTable> warehouseProductBatchTableQuery,
        IQueryRepository<WarehouseProductBatchAllocation> warehouseProductBatchAllocationQuery,
        IQueryRepository<OrganizationConfig> organizationConfigQuery,
        IQueryRepository<PurchaseDoc> purchaseDocQuery,
        IQueryRepository<WarehouseTransferDoc> warehouseTransferDocQuery,
        IQueryRepository<InventoryAdjustmentDoc> inventoryAdjustmentDocQuery,
        IQueryRepository<OpeningInventory> openingInventoryQuery,
        IQueryRepository<SaleDocTable> saleDocTableQuery,
        IQueryRepository<RetailSaleDocTable> retailSaleDocTableQuery,
        ICommandRepository<WarehouseProduct> warehouseProductCommand,
        ICommandRepository<WarehouseProductTable> warehouseProductTableCommand,
        ICommandRepository<WarehouseProductMovement> warehouseProductMovementCommand,
        ICommandRepository<WarehouseProductBatch> warehouseProductBatchCommand,
        ICommandRepository<WarehouseProductBatchTable> warehouseProductBatchTableCommand,
        ICommandRepository<WarehouseProductBatchAllocation> warehouseProductBatchAllocationCommand,
        ICommandRepository<SaleDocProduct> saleDocProductCommand,
        ICommandRepository<SaleDocTable> saleDocTableCommand,
        ICommandRepository<SaleDocProductBatch> saleDocProductBatchCommand)
    {
        _queryBuilder = queryBuilder;
        _userContext = userContext;
        _productQuery = productQuery;
        _productTableQuery = productTableQuery;
        _warehouseProductQuery = warehouseProductQuery;
        _warehouseProductTableQuery = warehouseProductTableQuery;
        _warehouseProductMovementQuery = warehouseProductMovementQuery;
        _warehouseProductBatchQuery = warehouseProductBatchQuery;
        _warehouseProductBatchTableQuery = warehouseProductBatchTableQuery;
        _warehouseProductBatchAllocationQuery = warehouseProductBatchAllocationQuery;
        _organizationConfigQuery = organizationConfigQuery;
        _purchaseDocQuery = purchaseDocQuery;
        _warehouseTransferDocQuery = warehouseTransferDocQuery;
        _inventoryAdjustmentDocQuery = inventoryAdjustmentDocQuery;
        _openingInventoryQuery = openingInventoryQuery;
        _saleDocTableQuery = saleDocTableQuery;
        _retailSaleDocTableQuery = retailSaleDocTableQuery;
        _warehouseProductCommand = warehouseProductCommand;
        _warehouseProductTableCommand = warehouseProductTableCommand;
        _warehouseProductMovementCommand = warehouseProductMovementCommand;
        _warehouseProductBatchCommand = warehouseProductBatchCommand;
        _warehouseProductBatchTableCommand = warehouseProductBatchTableCommand;
        _warehouseProductBatchAllocationCommand = warehouseProductBatchAllocationCommand;
        _saleDocProductCommand = saleDocProductCommand;
        _saleDocTableCommand = saleDocTableCommand;
        _saleDocProductBatchCommand = saleDocProductBatchCommand;
    }

    public Task<Result> ApplyInventoryEntriesAsync(IReadOnlyCollection<InventoryMovementEntry> entries, CancellationToken ct = default) =>
        ApplyInventoryEntriesAsync(entries, null, ct);

    private async Task<Result> ApplyInventoryEntriesAsync(
        IReadOnlyCollection<InventoryMovementEntry> entries,
        IReadOnlyDictionary<InventoryMovementEntry, IReadOnlyList<ProductBatchAllocation>>? saleAllocations,
        CancellationToken ct)
    {
        if (entries.Count == 0)
            return Result.Success();

        var entryList = entries.ToList();
        var operationValidation = ValidateOperations(entryList);
        if (!operationValidation.IsSuccess)
            return operationValidation;

        var products = await GetProductSnapshotsAsync(entryList.Select(x => x.ProductId), ct);
        if (products.Count != entryList.Select(x => x.ProductId).Distinct().Count())
            return Result.Failure(WarehouseProductErrors.ProductNotFound(entryList.Select(x => x.ProductId).First(x => !products.ContainsKey(x)), _userContext.LanguageId));

        var productUnitIds = products.ToDictionary(x => x.Key, x => x.Value.UnitId);
        var movementResult = await ApplyMovementsAndBatchesAsync(entryList, saleAllocations, ct);
        if (!movementResult.IsSuccess)
            return movementResult;

        var productTableEntries = entryList.Where(x => x.ProductTableId.HasValue).ToList();
        var productTablesResult = await GetProductTablesAsync(productTableEntries, ct);
        if (!productTablesResult.IsSuccess)
            return Result.Failure(productTablesResult.Error);

        var productTablesById = productTablesResult.Value;
        var warehouseProductTables = await GetWarehouseProductTablesAsync(productTableEntries, ct);
        var warehouseProductTablesById = warehouseProductTables.ToDictionary(x => x.ProductTableId);
        var createdWarehouseProductTables = new List<WarehouseProductTable>();
        var updatedWarehouseProductTables = new HashSet<WarehouseProductTable>();
        var deletedWarehouseProductTables = new List<WarehouseProductTable>();
        var changes = new List<WarehouseProductBalanceChange>();
        var processedEntries = new HashSet<InventoryMovementEntry>();

        foreach (var group in GetTransferGroups(productTableEntries))
        {
            var sourceEntry = group.Single(x => x.OperationTypeId == OperationTypeIdConst.OUT);
            var destinationEntry = group.Single(x => x.OperationTypeId == OperationTypeIdConst.IN);

            if (!warehouseProductTablesById.TryGetValue(group.Key, out var warehouseProductTable))
                return Result.Failure(WarehouseProductErrors.ProductTableNotFound(group.Key, _userContext.LanguageId));

            var validation = ValidateWarehouseProductTable(
                warehouseProductTable,
                sourceEntry.ProductTableId!.Value,
                sourceEntry.ProductId,
                sourceEntry.WarehouseId,
                ProductTableStatusIdConst.IN_STOCK);
            if (!validation.IsSuccess)
                return validation;

            warehouseProductTable.WarehouseId = destinationEntry.WarehouseId;
            warehouseProductTable.StatusId = ProductTableStatusIdConst.IN_STOCK;
            updatedWarehouseProductTables.Add(warehouseProductTable);

            changes.Add(ToQuantityChange(sourceEntry));
            changes.Add(ToQuantityChange(destinationEntry));
            processedEntries.Add(sourceEntry);
            processedEntries.Add(destinationEntry);
        }

        var receivedDates = await GetReceivedDatesAsync(
            productTableEntries
                .Where(x => x.OperationTypeId == OperationTypeIdConst.IN && x.OriginalMovementId.HasValue)
                .Select(x => x.ProductTableId!.Value),
            ct);

        foreach (var entry in entryList.Where(x => !processedEntries.Contains(x)))
        {
            if (!entry.ProductTableId.HasValue)
            {
                changes.Add(ToQuantityChange(entry));
                continue;
            }

            var productTableId = entry.ProductTableId.Value;
            if (!productTablesById.TryGetValue(productTableId, out var productId) || productId != entry.ProductId)
                return Result.Failure(WarehouseProductErrors.ProductTableProductMismatch(productTableId, entry.ProductId, _userContext.LanguageId));

            if (entry.OperationTypeId == OperationTypeIdConst.IN)
            {
                if (warehouseProductTablesById.ContainsKey(productTableId))
                    return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                        productTableId,
                        warehouseProductTablesById[productTableId].StatusId,
                        _userContext.LanguageId));

                var warehouseProductTable = new WarehouseProductTable
                {
                    ProductTableId = productTableId,
                    WarehouseId = entry.WarehouseId,
                    StatusId = ProductTableStatusIdConst.IN_STOCK,
                    ReceivedDate = receivedDates.GetValueOrDefault(productTableId, entry.DocDate),
                    CreatedDate = DateTime.Now
                };

                createdWarehouseProductTables.Add(warehouseProductTable);
                warehouseProductTablesById[productTableId] = warehouseProductTable;
                changes.Add(ToQuantityChange(entry));
                continue;
            }

            if (!warehouseProductTablesById.TryGetValue(productTableId, out var existingWarehouseProductTable))
                return Result.Failure(WarehouseProductErrors.ProductTableNotFound(productTableId, _userContext.LanguageId));

            var tableValidation = ValidateWarehouseProductTable(
                existingWarehouseProductTable,
                productTableId,
                entry.ProductId,
                entry.WarehouseId,
                ProductTableStatusIdConst.IN_STOCK,
                ProductTableStatusIdConst.RESERVED);
            if (!tableValidation.IsSuccess)
                return tableValidation;

            changes.Add(ToQuantityChange(entry, existingWarehouseProductTable.StatusId == ProductTableStatusIdConst.RESERVED ? -entry.Quantity : 0m));
            deletedWarehouseProductTables.Add(existingWarehouseProductTable);
            warehouseProductTablesById.Remove(productTableId);
        }

        var balanceResult = await ApplyWarehouseProductChangesAsync(changes, productUnitIds, ct);
        if (!balanceResult.IsSuccess)
            return balanceResult;

        if (createdWarehouseProductTables.Count > 0)
            await _warehouseProductTableCommand.CreateAsync(createdWarehouseProductTables, ct);
        if (updatedWarehouseProductTables.Count > 0)
            await _warehouseProductTableCommand.UpdateAsync(updatedWarehouseProductTables, ct);
        if (deletedWarehouseProductTables.Count > 0)
            await _warehouseProductTableCommand.DeleteAsync(deletedWarehouseProductTables, ct);

        return Result.Success();
    }

    public Task<Result> ReserveAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default) =>
        ApplyReservationAsync(warehouseId, items, [], reserve: true, ct);

    public Task<Result> ReserveAsync(
        int warehouseId,
        IReadOnlyCollection<WarehouseProductBalanceItem> items,
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct = default) =>
        ApplyReservationAsync(warehouseId, items, productTableIds, reserve: true, ct);

    public Task<Result> ReleaseReservedAsync(int warehouseId, IReadOnlyCollection<WarehouseProductBalanceItem> items, CancellationToken ct = default) =>
        ApplyReservationAsync(warehouseId, items, [], reserve: false, ct);

    public Task<Result> ReleaseReservedAsync(
        int warehouseId,
        IReadOnlyCollection<WarehouseProductBalanceItem> items,
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct = default) =>
        ApplyReservationAsync(warehouseId, items, productTableIds, reserve: false, ct);

    private async Task<Result> ApplyReservationAsync(
        int warehouseId,
        IReadOnlyCollection<WarehouseProductBalanceItem> items,
        IReadOnlyCollection<int> productTableIds,
        bool reserve,
        CancellationToken ct)
    {
        var invalidItem = items.FirstOrDefault(x => x.Quantity < 0m);
        if (invalidItem is not null)
            return Result.Failure(WarehouseProductErrors.InvalidQuantity(invalidItem.ProductId, invalidItem.Quantity, _userContext.LanguageId));

        var distinctProductTableIds = productTableIds.Distinct().ToList();

        var updatedWarehouseProductTables = new List<WarehouseProductTable>();
        if (distinctProductTableIds.Count > 0)
        {
            var tables = await _warehouseProductTableQuery.GetAllAsync(
                _queryBuilder.For<WarehouseProductTable>()
                    .Where(x => distinctProductTableIds.Contains(x.ProductTableId))
                    .Build(),
                ct);

            if (tables.Count != distinctProductTableIds.Count)
            {
                var foundIds = tables.Select(x => x.ProductTableId).ToHashSet();
                return Result.Failure(WarehouseProductErrors.ProductTableNotFound(
                    distinctProductTableIds.First(x => !foundIds.Contains(x)),
                    _userContext.LanguageId));
            }

            var productTablesById = await GetProductTableProductIdsAsync(distinctProductTableIds, ct);
            if (productTablesById.Count != distinctProductTableIds.Count)
                return Result.Failure(WarehouseProductErrors.ProductTableNotFound(
                    distinctProductTableIds.First(id => !productTablesById.ContainsKey(id)),
                    _userContext.LanguageId));

            var expectedStatus = reserve ? ProductTableStatusIdConst.IN_STOCK : ProductTableStatusIdConst.RESERVED;
            var nextStatus = reserve ? ProductTableStatusIdConst.RESERVED : ProductTableStatusIdConst.IN_STOCK;

            foreach (var table in tables)
            {
                await _warehouseProductTableCommand.ReloadAsync(table, ct);
                if (table.WarehouseId != warehouseId || table.StatusId != expectedStatus)
                    return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(
                        table.ProductTableId,
                        table.StatusId,
                        _userContext.LanguageId));

                table.StatusId = nextStatus;
                updatedWarehouseProductTables.Add(table);
            }
        }

        var productUnitIds = await GetProductUnitIdsAsync(items.Select(x => x.ProductId), ct);
        if (productUnitIds.Count != items.Select(x => x.ProductId).Distinct().Count())
            return Result.Failure(WarehouseProductErrors.ProductNotFound(
                items.Select(x => x.ProductId).First(x => !productUnitIds.ContainsKey(x)),
                _userContext.LanguageId));

        var changes = items.Select(item => new WarehouseProductBalanceChange(
            warehouseId,
            item.ProductId,
            item.UnitId,
            QuantityDelta: 0m,
            ReservedQuantityDelta: reserve ? item.Quantity : -item.Quantity,
            BlockedQuantityDelta: 0m)).ToList();

        var balanceResult = await ApplyWarehouseProductChangesAsync(changes, productUnitIds, ct);
        if (!balanceResult.IsSuccess)
            return balanceResult;

        if (updatedWarehouseProductTables.Count > 0)
            await _warehouseProductTableCommand.UpdateAsync(updatedWarehouseProductTables, ct);

        return Result.Success();
    }

    private Result ValidateOperations(IReadOnlyCollection<InventoryMovementEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.Quantity < 0m)
                return Result.Failure(WarehouseProductErrors.InvalidQuantity(entry.ProductId, entry.Quantity, _userContext.LanguageId));

            if (entry.OperationTypeId is not (OperationTypeIdConst.IN or OperationTypeIdConst.OUT))
                return Result.Failure(WarehouseProductErrors.UnsupportedOperation(entry.OperationTypeId, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private async Task<Result> ApplyMovementsAndBatchesAsync(
        IReadOnlyCollection<InventoryMovementEntry> entries,
        IReadOnlyDictionary<InventoryMovementEntry, IReadOnlyList<ProductBatchAllocation>>? saleAllocations,
        CancellationToken ct)
    {
        var movementGroups = BuildWarehouseMovementGroups(entries);
        var movementsByEntry = movementGroups
            .SelectMany(group => group.Entries.Select(entry => new { Entry = entry, group.Movement }))
            .ToDictionary(item => item.Entry, item => item.Movement);
        var entriesByMovement = movementGroups.ToDictionary(group => group.Movement, group => group.Entries);
        var receiptUnitCostsByMovement = movementGroups.ToDictionary(group => group.Movement, group => group.UnitCost);

        await _warehouseProductMovementCommand.CreateAsync(
            movementGroups.Select(group => group.Movement),
            ct);

        var receiptDocumentNumbers = await GetReceiptDocumentNumbersAsync(
            movementGroups.Select(group => group.Movement).Where(movement => movement.MovementSign == 1),
            ct);

        var batchEntries = entries.ToList();
        if (batchEntries.Count == 0)
            return Result.Success();

        var valuationMethods = await GetInventoryValuationMethodsAsync(
            batchEntries.Select(entry => entry.OrganizationId),
            ct);
        var processedEntries = new HashSet<InventoryMovementEntry>();

        foreach (var transferGroup in GetBatchTransferGroups(batchEntries))
        {
            var sourceEntries = transferGroup
                .Where(entry => entry.OperationTypeId == OperationTypeIdConst.OUT)
                .ToList();
            var destinationEntries = transferGroup
                .Where(entry => entry.OperationTypeId == OperationTypeIdConst.IN)
                .ToList();

            foreach (var sourceEntry in sourceEntries)
            {
                var issueResult = await ApplyIssueMovementAsync(
                    sourceEntry,
                    movementsByEntry[sourceEntry],
                    valuationMethods[sourceEntry.OrganizationId],
                    ct);
                if (!issueResult.IsSuccess)
                    return issueResult;
            }

            var sourceAmountByProductTableId = sourceEntries
                .Where(entry => entry.ProductTableId.HasValue)
                .ToDictionary(entry => entry.ProductTableId!.Value, entry => entry.Amount);
            var sourceAmount = sourceEntries.Sum(entry => entry.Amount);
            var destinationMovement = movementsByEntry[destinationEntries[0]];
            receiptUnitCostsByMovement[destinationMovement] = destinationMovement.Quantity == 0m
                ? 0m
                : sourceAmount / destinationMovement.Quantity;

            if (destinationEntries.Count > 1 &&
                destinationEntries.All(entry => entry.OriginalMovementId.HasValue))
            {
                var receiptResult = await ApplyReceiptMovementAsync(
                    CreateAggregatedEntry(destinationEntries),
                    destinationMovement,
                    receiptUnitCostsByMovement[destinationMovement],
                    receiptDocumentNumbers,
                    ct);
                if (!receiptResult.IsSuccess)
                    return receiptResult;
            }
            else
            {
                foreach (var destinationEntry in destinationEntries)
                {
                    destinationEntry.Amount = destinationEntry.ProductTableId.HasValue
                        ? sourceAmountByProductTableId[destinationEntry.ProductTableId.Value]
                        : sourceAmount;

                    var receiptResult = await ApplyReceiptMovementAsync(
                        destinationEntry,
                        destinationMovement,
                        receiptUnitCostsByMovement[destinationMovement],
                        receiptDocumentNumbers,
                        ct);
                    if (!receiptResult.IsSuccess)
                        return receiptResult;
                }
            }

            processedEntries.UnionWith(sourceEntries);
            processedEntries.UnionWith(destinationEntries);
        }

        var processedIssueReversalMovements = new HashSet<WarehouseProductMovement>();
        foreach (var entry in batchEntries.Where(entry => !processedEntries.Contains(entry)))
        {
            var movement = movementsByEntry[entry];
            Result result;
            if (entry.OperationTypeId == OperationTypeIdConst.OUT &&
                saleAllocations is not null &&
                saleAllocations.TryGetValue(entry, out var plannedAllocations))
            {
                result = await ApplySaleIssueMovementAsync(entry, movement, plannedAllocations, ct);
            }
            else if (entry.OperationTypeId == OperationTypeIdConst.IN &&
                      entry.OriginalMovementId.HasValue &&
                     entriesByMovement[movement].Count > 1)
            {
                if (!processedIssueReversalMovements.Add(movement))
                    continue;

                result = await ApplyReceiptMovementAsync(
                    CreateAggregatedEntry(entriesByMovement[movement]),
                    movement,
                    receiptUnitCostsByMovement[movement],
                    receiptDocumentNumbers,
                    ct);
            }
            else
            {
                result = entry.OperationTypeId == OperationTypeIdConst.OUT
                    ? await ApplyIssueMovementAsync(entry, movement, valuationMethods[entry.OrganizationId], ct)
                    : await ApplyReceiptMovementAsync(entry, movement, receiptUnitCostsByMovement[movement],
                        receiptDocumentNumbers,
                        ct);
            }
            if (!result.IsSuccess)
                return result;
        }

        await _warehouseProductMovementCommand.UpdateAsync(
            movementGroups.Select(group => group.Movement),
            ct);
        return Result.Success();
    }

    private static List<WarehouseMovementGroup> BuildWarehouseMovementGroups(
        IReadOnlyCollection<InventoryMovementEntry> entries)
    {
        var now = DateTime.Now;
        return entries
            .GroupBy(entry =>
            {
                var separateProductTableMovement = entry.ProductTableId.HasValue
                    && entry.DocumentTypeId is not (DocumentTypeIdConst.FARECEIPT or DocumentTypeIdConst.PURCHASE);

                return new WarehouseMovementGroupKey(
                    entry.OrganizationId,
                    entry.WarehouseId,
                    entry.ProductId,
                    entry.DocumentTypeId,
                    entry.DocumentId,
                    entry.OperationTypeId,
                    separateProductTableMovement ? null : entry.SourceLineId,
                    separateProductTableMovement);
            })
            .Select(group =>
            {
                var groupedEntries = group.ToList();
                var firstEntry = groupedEntries[0];
                var quantity = groupedEntries.Sum(entry => entry.Quantity);
                var amount = groupedEntries.Sum(entry => entry.Amount);
                return new WarehouseMovementGroup(
                    groupedEntries,
                    new WarehouseProductMovement
                    {
                        OrganizationId = firstEntry.OrganizationId,
                        WarehouseId = firstEntry.WarehouseId,
                        ProductId = firstEntry.ProductId,
                        DocumentTypeId = firstEntry.DocumentTypeId,
                        DocumentId = firstEntry.DocumentId,
                        DocumentLineId = firstEntry.SourceLineId,
                        Quantity = quantity,
                        MovementSign = ToMovementSign(firstEntry.OperationTypeId),
                        MovementDate = firstEntry.DocDate,
                        CreatedDate = now
                    },
                    firstEntry.OperationTypeId == OperationTypeIdConst.IN && quantity != 0m
                        ? amount / quantity
                        : null);
            })
            .ToList();
    }

    private static InventoryMovementEntry CreateAggregatedEntry(IReadOnlyCollection<InventoryMovementEntry> entries)
    {
        var firstEntry = entries.First();
        return new InventoryMovementEntry
        {
            OrganizationId = firstEntry.OrganizationId,
            DocumentTypeId = firstEntry.DocumentTypeId,
            DocumentId = firstEntry.DocumentId,
            WarehouseId = firstEntry.WarehouseId,
            ProductId = firstEntry.ProductId,
            OperationTypeId = firstEntry.OperationTypeId,
            Quantity = entries.Sum(entry => entry.Quantity),
            Amount = entries.Sum(entry => entry.Amount),
            DocDate = firstEntry.DocDate,
            OriginalMovementId = firstEntry.OriginalMovementId
        };
    }

    private async Task<Result> ApplyIssueMovementAsync(
        InventoryMovementEntry entry,
        WarehouseProductMovement issueMovement,
        string valuationMethod,
        CancellationToken ct)
    {
        if (entry.OriginalMovementId.HasValue)
            return await ApplyReceiptReversalAsync(entry, issueMovement, ct);

        var batches = await GetAvailableBatchesAsync(
            entry.OrganizationId,
            entry.WarehouseId,
            entry.ProductId,
            entry.ProductTableId,
            valuationMethod == InventoryValuationMethodConst.LIFO,
            ct);
        var availableQuantity = batches.Sum(batch => batch.RemainingQuantity);
        if (availableQuantity < entry.Quantity)
            return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                entry.WarehouseId,
                entry.ProductId,
                entry.Quantity,
                availableQuantity,
                _userContext.LanguageId));

        var averageUnitCost = valuationMethod == InventoryValuationMethodConst.AVERAGE
            ? CalculateAverageUnitCost(batches)
            : 0m;
        var quantityToAllocate = entry.Quantity;
        var totalCost = 0m;

        foreach (var batch in batches)
        {
            if (quantityToAllocate == 0m)
                break;

            var quantity = Math.Min(quantityToAllocate, batch.RemainingQuantity);
            var unitCost = valuationMethod == InventoryValuationMethodConst.AVERAGE
                ? averageUnitCost
                : batch.UnitCost ?? 0m;

            batch.RemainingQuantity -= quantity;
            await _warehouseProductBatchCommand.UpdateAsync(batch, ct);
            await AddBatchAllocationAsync(issueMovement, batch, quantity, unitCost, ct);
            totalCost += quantity * unitCost;
            quantityToAllocate -= quantity;
        }

        entry.Amount = totalCost;
        return Result.Success();
    }

    private async Task<Result> ApplyReceiptMovementAsync(
        InventoryMovementEntry entry,
        WarehouseProductMovement receiptMovement,
        decimal? receiptUnitCost,
        IReadOnlyDictionary<(short DocumentTypeId, long DocumentId), string> receiptDocumentNumbers,
        CancellationToken ct)
    {
        if (entry.OriginalMovementId.HasValue)
            return await ApplyIssueReversalAsync(entry, ct);

        var batch = await _warehouseProductBatchQuery.GetAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(item => item.ReceiptMovementId == receiptMovement.Id)
                .Build(),
            ct);
        if (batch is null)
        {
            var unitCost = receiptUnitCost ?? 0m;
            if (unitCost == 0m && receiptMovement.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT)
            {
                unitCost = await GetWeightedUnitCostAsync(
                    receiptMovement.OrganizationId,
                    receiptMovement.WarehouseId,
                    receiptMovement.ProductId,
                    ct);
                entry.Amount = unitCost * entry.Quantity;
            }

            if (!receiptDocumentNumbers.TryGetValue(
                    (receiptMovement.DocumentTypeId, receiptMovement.DocumentId),
                    out var batchNumber))
            {
                return Result.Failure(WarehouseProductErrors.ReceiptDocumentNotFound(
                    receiptMovement.DocumentTypeId,
                    receiptMovement.DocumentId,
                    _userContext.LanguageId));
            }

            batch = new WarehouseProductBatch
            {
                OrganizationId = receiptMovement.OrganizationId,
                WarehouseId = receiptMovement.WarehouseId,
                ProductId = receiptMovement.ProductId,
                ReceiptMovementId = receiptMovement.Id,
                ReceiptMovement = receiptMovement,
                BatchNumber = batchNumber,
                InitialQuantity = receiptMovement.Quantity,
                RemainingQuantity = receiptMovement.Quantity,
                UnitCost = unitCost,
                ReceivedDate = receiptMovement.MovementDate,
                CreatedDate = DateTime.Now
            };

            await _warehouseProductBatchCommand.CreateAsync(batch, ct);
        }

        if (entry.ProductTableId.HasValue &&
            !await _warehouseProductBatchTableQuery.AnyAsync(
                link => link.BatchId == batch.Id && link.ProductTableId == entry.ProductTableId.Value,
                ct))
        {
            await _warehouseProductBatchTableCommand.CreateAsync(new WarehouseProductBatchTable
            {
                BatchId = batch.Id,
                ProductTableId = entry.ProductTableId.Value,
                CreatedDate = DateTime.Now
            }, ct);
        }

        return Result.Success();
    }
    private async Task<Result> ApplyReceiptReversalAsync(
        InventoryMovementEntry reversalEntry,
        WarehouseProductMovement issueMovement,
        CancellationToken ct)
    {
        var originalMovementId = reversalEntry.OriginalMovementId.GetValueOrDefault();
        var originalMovement = await _warehouseProductMovementQuery.GetAsync(
            _queryBuilder.For<WarehouseProductMovement>()
                .Where(item => item.Id == originalMovementId)
                .Build(),
            ct);
        if (originalMovement == null)
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(originalMovementId, _userContext.LanguageId));

        var batch = await _warehouseProductBatchQuery.GetAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(item => item.ReceiptMovementId == originalMovement.Id)
                .Build(),
            ct);
        if (batch == null)
            return Result.Failure(WarehouseProductErrors.OriginalBatchNotFound(originalMovement.Id, _userContext.LanguageId));

        if (batch.RemainingQuantity < reversalEntry.Quantity)
            return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                reversalEntry.WarehouseId,
                reversalEntry.ProductId,
                reversalEntry.Quantity,
                batch.RemainingQuantity,
                _userContext.LanguageId));

        var unitCost = batch.UnitCost ?? 0m;
        batch.RemainingQuantity -= reversalEntry.Quantity;
        await _warehouseProductBatchCommand.UpdateAsync(batch, ct);
        await AddBatchAllocationAsync(issueMovement, batch, reversalEntry.Quantity, unitCost, ct);
        reversalEntry.Amount = reversalEntry.Quantity * unitCost;
        return Result.Success();
    }

    private async Task<Result> ApplyIssueReversalAsync(
        InventoryMovementEntry reversalEntry,
        CancellationToken ct)
    {
        var originalMovementId = reversalEntry.OriginalMovementId.GetValueOrDefault();
        var originalMovement = await _warehouseProductMovementQuery.GetAsync(
            _queryBuilder.For<WarehouseProductMovement>()
                .Where(item => item.Id == originalMovementId)
                .Build(),
            ct);
        if (originalMovement == null)
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(originalMovementId, _userContext.LanguageId));

        var allocations = await _warehouseProductBatchAllocationQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatchAllocation>()
                .Where(item => item.IssueMovementId == originalMovement.Id)
                .Build(),
            ct);
        var isMarkingDrivenSale = originalMovement.DocumentTypeId is
            DocumentTypeIdConst.SALE or DocumentTypeIdConst.RETAIL_SALE;
        if (allocations.Count == 0)
        {
            if (!isMarkingDrivenSale)
                return Result.Failure(WarehouseProductErrors.OriginalAllocationNotFound(originalMovement.Id, _userContext.LanguageId));

            reversalEntry.Amount = 0m;
            return Result.Success();
        }

        var allocatedQuantity = allocations.Sum(item => item.Quantity);
        if (isMarkingDrivenSale
                ? allocatedQuantity > reversalEntry.Quantity
                : allocatedQuantity != reversalEntry.Quantity)
        {
            return Result.Failure(WarehouseProductErrors.OriginalAllocationNotFound(originalMovement.Id, _userContext.LanguageId));
        }

        var batchIds = allocations.Select(allocation => allocation.BatchId).Distinct().ToList();
        var batches = await _warehouseProductBatchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(batch => batchIds.Contains(batch.Id))
                .Build(),
            ct);
        var batchesById = batches.ToDictionary(batch => batch.Id);
        if (batchesById.Count != batchIds.Count)
            return Result.Failure(WarehouseProductErrors.OriginalAllocationNotFound(originalMovement.Id, _userContext.LanguageId));

        foreach (var allocation in allocations)
            batchesById[allocation.BatchId].RemainingQuantity += allocation.Quantity;

        await _warehouseProductBatchCommand.UpdateAsync(batchesById.Values, ct);
        reversalEntry.Amount = allocations.Sum(item => item.Quantity * (item.UnitCost ?? 0m));
        return Result.Success();
    }

    private async Task<List<WarehouseProductBatch>> GetAvailableBatchesAsync(
        int organizationId,
        int warehouseId,
        int productId,
        int? productTableId,
        bool lifo,
        CancellationToken ct)
    {
        IReadOnlyCollection<long>? batchIds = null;
        if (productTableId.HasValue)
        {
            batchIds = (await _warehouseProductBatchTableQuery.GetAllAsync(
                    _queryBuilder.For<WarehouseProductBatchTable>()
                        .Where(link => link.ProductTableId == productTableId.Value)
                        .As(link => link.BatchId)
                        .Build(),
                    ct))
                .Distinct()
                .ToList();
            if (batchIds.Count == 0)
                return [];
        }

        var batches = await _warehouseProductBatchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(item => item.OrganizationId == organizationId &&
                               item.WarehouseId == warehouseId &&
                               item.ProductId == productId &&
                               item.RemainingQuantity > 0m &&
                               (!productTableId.HasValue || batchIds!.Contains(item.Id)))
                .Build(),
            ct);

        return lifo
            ? batches.OrderByDescending(item => item.ReceivedDate).ThenByDescending(item => item.Id).ToList()
            : batches.OrderBy(item => item.ReceivedDate).ThenBy(item => item.Id).ToList();
    }

    private async Task<decimal> GetWeightedUnitCostAsync(int organizationId, int warehouseId, int productId, CancellationToken ct)
    {
        var batches = await _warehouseProductBatchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(item => item.OrganizationId == organizationId &&
                               item.WarehouseId == warehouseId &&
                               item.ProductId == productId &&
                               item.RemainingQuantity > 0m)
                .As(item => new WarehouseProductBatchCost(item.RemainingQuantity, item.UnitCost))
                .Build(),
            ct);

        var quantity = batches.Sum(item => item.RemainingQuantity);
        return quantity == 0m
            ? 0m
            : batches.Sum(item => item.RemainingQuantity * (item.UnitCost ?? 0m)) / quantity;
    }

    private async Task<Dictionary<int, string>> GetInventoryValuationMethodsAsync(IEnumerable<int> organizationIds, CancellationToken ct)
    {
        var ids = organizationIds.Distinct().ToList();
        var configured = await _organizationConfigQuery.GetAllAsync(
            _queryBuilder.For<OrganizationConfig>()
                .Where(config => ids.Contains(config.OrganizationId))
                .As(config => new OrganizationValuationMethod(config.OrganizationId, config.InventoryValuationMethod))
                .Build(),
            ct);
        var configuredByOrganization = configured.ToDictionary(config => config.OrganizationId, config => config.InventoryValuationMethod);

        return ids.ToDictionary(
            id => id,
            id => NormalizeInventoryValuationMethod(configuredByOrganization.GetValueOrDefault(id)));
    }

    private async Task AddBatchAllocationAsync(
        WarehouseProductMovement issueMovement,
        WarehouseProductBatch batch,
        decimal quantity,
        decimal unitCost,
        CancellationToken ct)
    {
        var allocations = await _warehouseProductBatchAllocationQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatchAllocation>()
                .Where(allocation => allocation.IssueMovementId == issueMovement.Id &&
                                     allocation.BatchId == batch.Id)
                .Build(),
            ct);
        var existingAllocation = allocations.SingleOrDefault();
        if (existingAllocation is not null)
        {
            existingAllocation.Quantity += quantity;
            await _warehouseProductBatchAllocationCommand.UpdateAsync(existingAllocation, ct);
            return;
        }

        await _warehouseProductBatchAllocationCommand.CreateAsync(new WarehouseProductBatchAllocation
        {
            IssueMovementId = issueMovement.Id,
            BatchId = batch.Id,
            Quantity = quantity,
            UnitCost = unitCost,
            CreatedDate = DateTime.Now
        }, ct);
    }

    private static IEnumerable<IGrouping<string, InventoryMovementEntry>> GetBatchTransferGroups(IEnumerable<InventoryMovementEntry> entries) =>
        entries
            .Where(entry => entry.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER)
            .GroupBy(entry => $"{entry.DocumentId}:{entry.ProductId}")
            .Where(group => group.Any(entry => entry.OperationTypeId == OperationTypeIdConst.IN) &&
                            group.Any(entry => entry.OperationTypeId == OperationTypeIdConst.OUT) &&
                            group.Where(entry => entry.OperationTypeId == OperationTypeIdConst.IN).Sum(entry => entry.Quantity) ==
                            group.Where(entry => entry.OperationTypeId == OperationTypeIdConst.OUT).Sum(entry => entry.Quantity));

    private static short ToMovementSign(short operationTypeId) =>
        operationTypeId == OperationTypeIdConst.IN ? (short)1 : (short)-1;

    private static decimal CalculateAverageUnitCost(IEnumerable<WarehouseProductBatch> batches)
    {
        var quantity = batches.Sum(batch => batch.RemainingQuantity);
        return quantity == 0m
            ? 0m
            : batches.Sum(batch => batch.RemainingQuantity * (batch.UnitCost ?? 0m)) / quantity;
    }

    private static string NormalizeInventoryValuationMethod(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            InventoryValuationMethodConst.LIFO => InventoryValuationMethodConst.LIFO,
            InventoryValuationMethodConst.AVERAGE => InventoryValuationMethodConst.AVERAGE,
            _ => InventoryValuationMethodConst.FIFO
        };

    private async Task<Dictionary<int, WarehouseProductSnapshot>> GetProductSnapshotsAsync(IEnumerable<int> productIds, CancellationToken ct)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, WarehouseProductSnapshot>();

        var products = await _productQuery.GetAllAsync(
            _queryBuilder.For<Product>()
                .Where(product => ids.Contains(product.Id))
                .As(product => new WarehouseProductSnapshot(product.Id, product.UnitId))
                .Build(),
            ct);
        return products.ToDictionary(product => product.Id);
    }
    private async Task<Result<Dictionary<int, int>>> GetProductTablesAsync(
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct)
    {
        var productTableIds = entries.Select(x => x.ProductTableId!.Value).Distinct().ToList();
        if (productTableIds.Count == 0)
            return Result.Success(new Dictionary<int, int>());

        var rows = await _productTableQuery.GetAllAsync(
            _queryBuilder.For<ProductTable>()
                .Where(x => productTableIds.Contains(x.Id))
                .As(x => new ProductTableProduct(x.Id, x.ProductId))
                .Build(),
            ct);

        var byId = rows.ToDictionary(x => x.Id, x => x.ProductId);
        var missingId = productTableIds.FirstOrDefault(x => !byId.ContainsKey(x));
        return missingId == 0
            ? Result.Success(byId)
            : Result.Failure<Dictionary<int, int>>(WarehouseProductErrors.ProductTableNotFound(missingId, _userContext.LanguageId));
    }

    private async Task<List<WarehouseProductTable>> GetWarehouseProductTablesAsync(
        IReadOnlyCollection<InventoryMovementEntry> entries,
        CancellationToken ct)
    {
        var productTableIds = entries.Select(x => x.ProductTableId!.Value).Distinct().ToList();
        return productTableIds.Count == 0
            ? []
            : await _warehouseProductTableQuery.GetAllAsync(
                _queryBuilder.For<WarehouseProductTable>()
                    .Where(x => productTableIds.Contains(x.ProductTableId))
                    .Build(),
                ct);
    }

    private async Task<Dictionary<int, int>> GetProductTableProductIdsAsync(
        IReadOnlyCollection<int> productTableIds,
        CancellationToken ct)
    {
        var ids = productTableIds.Distinct().ToList();
        var productTables = await _productTableQuery.GetAllAsync(
            _queryBuilder.For<ProductTable>()
                .Where(table => ids.Contains(table.Id))
                .As(table => new ProductTableProduct(table.Id, table.ProductId))
                .Build(),
            ct);
        return productTables.ToDictionary(table => table.Id, table => table.ProductId);
    }

    private async Task<Dictionary<int, DateTime>> GetReceivedDatesAsync(IEnumerable<int> productTableIds, CancellationToken ct)
    {
        var ids = productTableIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DateTime>();

        var batchLinks = await _warehouseProductBatchTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatchTable>()
                .Where(link => ids.Contains(link.ProductTableId))
                .As(link => new ProductTableBatchLink(link.ProductTableId, link.BatchId))
                .Build(),
            ct);
        if (batchLinks.Count == 0)
            return new Dictionary<int, DateTime>();

        var batchIds = batchLinks.Select(link => link.BatchId).Distinct().ToList();
        var batches = await _warehouseProductBatchQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductBatch>()
                .Where(batch => batchIds.Contains(batch.Id))
                .As(batch => new BatchReceivedDate(batch.Id, batch.ReceivedDate))
                .Build(),
            ct);
        var receivedDateByBatchId = batches.ToDictionary(batch => batch.BatchId, batch => batch.ReceivedDate);

        return batchLinks
            .Where(link => receivedDateByBatchId.ContainsKey(link.BatchId))
            .GroupBy(link => link.ProductTableId)
            .ToDictionary(
                group => group.Key,
                group => group.Min(link => receivedDateByBatchId[link.BatchId]));
    }

    private async Task<Dictionary<(short DocumentTypeId, long DocumentId), string>> GetReceiptDocumentNumbersAsync(
        IEnumerable<WarehouseProductMovement> movements,
        CancellationToken ct)
    {
        var documentKeys = movements
            .Where(movement => movement.MovementSign == 1)
            .Select(movement => (movement.DocumentTypeId, movement.DocumentId))
            .Distinct()
            .ToList();
        var result = new Dictionary<(short DocumentTypeId, long DocumentId), string>();

        var purchaseIds = documentKeys
            .Where(key => key.DocumentTypeId == DocumentTypeIdConst.PURCHASE)
            .Select(key => key.DocumentId)
            .ToList();
        if (purchaseIds.Count > 0)
        {
            var documents = await _purchaseDocQuery.GetAllAsync(
                _queryBuilder.For<PurchaseDoc>()
                    .Where(document => purchaseIds.Contains(document.Id))
                    .As(document => new DocumentNumber(document.Id, document.DocNumber))
                    .Build(),
                ct);
            foreach (var document in documents)
                result[(DocumentTypeIdConst.PURCHASE, document.Id)] = document.DocNumber;
        }

        var transferIds = documentKeys
            .Where(key => key.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER)
            .Select(key => key.DocumentId)
            .ToList();
        if (transferIds.Count > 0)
        {
            var documents = await _warehouseTransferDocQuery.GetAllAsync(
                _queryBuilder.For<WarehouseTransferDoc>()
                    .Where(document => transferIds.Contains(document.Id))
                    .As(document => new DocumentNumber(document.Id, document.DocNumber))
                    .Build(),
                ct);
            foreach (var document in documents)
                result[(DocumentTypeIdConst.WAREHOUSETRANSFER, document.Id)] = document.DocNumber;
        }

        var adjustmentIds = documentKeys
            .Where(key => key.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT)
            .Select(key => key.DocumentId)
            .ToList();
        if (adjustmentIds.Count > 0)
        {
            var documents = await _inventoryAdjustmentDocQuery.GetAllAsync(
                _queryBuilder.For<InventoryAdjustmentDoc>()
                    .Where(document => adjustmentIds.Contains(document.Id))
                    .As(document => new DocumentNumber(document.Id, document.DocNumber))
                    .Build(),
                ct);
            foreach (var document in documents)
                result[(DocumentTypeIdConst.INVENTORYADJUSTMENT, document.Id)] = document.DocNumber;
        }

        var openingInventoryIds = documentKeys
            .Where(key => key.DocumentTypeId == DocumentTypeIdConst.OPENINGINVENTORY)
            .Select(key => key.DocumentId)
            .ToList();
        if (openingInventoryIds.Count > 0)
        {
            var documents = await _openingInventoryQuery.GetAllAsync(
                _queryBuilder.For<OpeningInventory>()
                    .Where(document => openingInventoryIds.Contains(document.Id))
                    .As(document => new DocumentNumber(document.Id, document.DocNumber))
                    .Build(),
                ct);
            foreach (var document in documents)
                result[(DocumentTypeIdConst.OPENINGINVENTORY, document.Id)] = document.DocNumber;
        }

        return result;
    }
    private static IEnumerable<IGrouping<int, InventoryMovementEntry>> GetTransferGroups(IReadOnlyCollection<InventoryMovementEntry> entries) =>
        entries
            .Where(x => x.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER)
            .GroupBy(x => x.ProductTableId!.Value)
            .Where(x => x.Count() == 2 &&
                        x.Count(entry => entry.OperationTypeId == OperationTypeIdConst.IN) == 1 &&
                        x.Count(entry => entry.OperationTypeId == OperationTypeIdConst.OUT) == 1);

    private Result ValidateWarehouseProductTable(
        WarehouseProductTable warehouseProductTable,
        int productTableId,
        int productId,
        int warehouseId,
        params short[] allowedStatuses)
    {
        if (warehouseProductTable.WarehouseId != warehouseId)
            return Result.Failure(WarehouseProductErrors.ProductTableWarehouseMismatch(productTableId, warehouseId, _userContext.LanguageId));

        if (!allowedStatuses.Contains(warehouseProductTable.StatusId))
            return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(productTableId, warehouseProductTable.StatusId, _userContext.LanguageId));

        return Result.Success();
    }

    private static WarehouseProductBalanceChange ToQuantityChange(InventoryMovementEntry entry, decimal reservedQuantityDelta = 0m) =>
        new(
            entry.WarehouseId,
            entry.ProductId,
            UnitId: null,
            QuantityDelta: entry.OperationTypeId == OperationTypeIdConst.IN ? entry.Quantity : -entry.Quantity,
            ReservedQuantityDelta: reservedQuantityDelta,
            BlockedQuantityDelta: 0m);

    private async Task<Dictionary<int, short>> GetProductUnitIdsAsync(IEnumerable<int> productIds, CancellationToken ct)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, short>();

        var products = await _productQuery.GetAllAsync(
            _queryBuilder.For<Product>()
                .Where(product => ids.Contains(product.Id))
                .As(product => new WarehouseProductSnapshot(product.Id, product.UnitId))
                .Build(),
            ct);
        return products.ToDictionary(product => product.Id, product => product.UnitId);
    }

    private async Task<Result> ApplyWarehouseProductChangesAsync(
        IReadOnlyCollection<WarehouseProductBalanceChange> changes,
        IReadOnlyDictionary<int, short> productUnitIds,
        CancellationToken ct)
    {
        var normalized = changes
            .Where(x => x.QuantityDelta != 0m || x.ReservedQuantityDelta != 0m || x.BlockedQuantityDelta != 0m)
            .GroupBy(x => new { x.WarehouseId, x.ProductId })
            .Select(group => new WarehouseProductBalanceChange(
                group.Key.WarehouseId,
                group.Key.ProductId,
                group.Select(x => x.UnitId).FirstOrDefault(x => x.HasValue),
                group.Sum(x => x.QuantityDelta),
                group.Sum(x => x.ReservedQuantityDelta),
                group.Sum(x => x.BlockedQuantityDelta)))
            .ToList();

        if (normalized.Count == 0)
            return Result.Success();

        var productIds = normalized.Select(x => x.ProductId).Distinct().ToList();
        var warehouseIds = normalized.Select(x => x.WarehouseId).Distinct().ToList();
        var existing = await _warehouseProductQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProduct>()
                .Where(x => warehouseIds.Contains(x.WarehouseId) && productIds.Contains(x.ProductId))
                .Build(),
            ct);
        var existingByKey = existing.ToDictionary(x => (x.WarehouseId, x.ProductId));
        var createdWarehouseProducts = new List<WarehouseProduct>();
        var updatedWarehouseProducts = new List<WarehouseProduct>();

        var missingKeys = normalized
            .Where(x => !existingByKey.ContainsKey((x.WarehouseId, x.ProductId)))
            .Select(x => (x.WarehouseId, x.ProductId))
            .Distinct()
            .ToList();
        var initialBalances = await GetInitialBalancesAsync(missingKeys, ct);
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
                warehouseProduct.AvailableQuantity = warehouseProduct.Quantity - warehouseProduct.ReservedQuantity - warehouseProduct.BlockedQuantity;

                createdWarehouseProducts.Add(warehouseProduct);
                existingByKey[key] = warehouseProduct;
            }

            var validation = ValidateChange(warehouseProduct, change);
            if (!validation.IsSuccess)
                return validation;

            warehouseProduct.Quantity += change.QuantityDelta;
            warehouseProduct.ReservedQuantity += change.ReservedQuantityDelta;
            warehouseProduct.BlockedQuantity += change.BlockedQuantityDelta;
            warehouseProduct.AvailableQuantity = warehouseProduct.Quantity - warehouseProduct.ReservedQuantity - warehouseProduct.BlockedQuantity;
            if (!createdWarehouseProducts.Contains(warehouseProduct))
                updatedWarehouseProducts.Add(warehouseProduct);
        }

        if (createdWarehouseProducts.Count > 0)
            await _warehouseProductCommand.CreateAsync(createdWarehouseProducts, ct);
        if (updatedWarehouseProducts.Count > 0)
            await _warehouseProductCommand.UpdateAsync(updatedWarehouseProducts.Distinct(), ct);

        return Result.Success();
    }

    private Result ValidateChange(WarehouseProduct warehouseProduct, WarehouseProductBalanceChange change)
    {
        if (change.QuantityDelta < 0m && warehouseProduct.Quantity < Math.Abs(change.QuantityDelta))
            return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.QuantityDelta),
                warehouseProduct.Quantity,
                _userContext.LanguageId));

        if (change.ReservedQuantityDelta < 0m && warehouseProduct.ReservedQuantity < Math.Abs(change.ReservedQuantityDelta))
            return Result.Failure(WarehouseProductErrors.NotEnoughReserved(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.ReservedQuantityDelta),
                warehouseProduct.ReservedQuantity,
                _userContext.LanguageId));

        if (change.BlockedQuantityDelta < 0m && warehouseProduct.BlockedQuantity < Math.Abs(change.BlockedQuantityDelta))
            return Result.Failure(WarehouseProductErrors.NotEnoughBlocked(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Abs(change.BlockedQuantityDelta),
                warehouseProduct.BlockedQuantity,
                _userContext.LanguageId));

        var availableQuantity = warehouseProduct.Quantity + change.QuantityDelta -
                                warehouseProduct.ReservedQuantity - change.ReservedQuantityDelta -
                                warehouseProduct.BlockedQuantity - change.BlockedQuantityDelta;
        return availableQuantity < 0m
            ? Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                warehouseProduct.WarehouseId,
                warehouseProduct.ProductId,
                Math.Max(-change.QuantityDelta, 0m) + Math.Max(change.ReservedQuantityDelta, 0m),
                warehouseProduct.AvailableQuantity,
                _userContext.LanguageId))
            : Result.Success();
    }

    private async Task<Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>> GetInitialBalancesAsync(
        IReadOnlyCollection<(int WarehouseId, int ProductId)> keys,
        CancellationToken ct)
    {
        if (keys.Count == 0)
            return new Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>();

        var warehouseIds = keys.Select(x => x.WarehouseId).Distinct().ToList();
        var productIds = keys.Select(x => x.ProductId).Distinct().ToList();
        var productTables = await _productTableQuery.GetAllAsync(
            _queryBuilder.For<ProductTable>()
                .Where(table => productIds.Contains(table.ProductId))
                .As(table => new ProductTableProduct(table.Id, table.ProductId))
                .Build(),
            ct);
        var productIdByProductTableId = productTables.ToDictionary(table => table.Id, table => table.ProductId);
        if (productIdByProductTableId.Count == 0)
            return new Dictionary<(int WarehouseId, int ProductId), WarehouseProductInitialBalance>();

        var productTableIds = productIdByProductTableId.Keys.ToList();
        var tables = await _warehouseProductTableQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductTable>()
                .Where(table => warehouseIds.Contains(table.WarehouseId) &&
                                productTableIds.Contains(table.ProductTableId) &&
                                (table.StatusId == ProductTableStatusIdConst.IN_STOCK ||
                                 table.StatusId == ProductTableStatusIdConst.RESERVED ||
                                 table.StatusId == ProductTableStatusIdConst.BLOCKED))
                .Build(),
            ct);

        return tables
            .Select(table => new WarehouseProductInitialBalanceRow(
                table.WarehouseId,
                productIdByProductTableId[table.ProductTableId],
                table.StatusId))
            .GroupBy(row => (row.WarehouseId, row.ProductId))
            .ToDictionary(
                group => group.Key,
                group => new WarehouseProductInitialBalance(
                    group.Count(),
                    group.Count(row => row.StatusId == ProductTableStatusIdConst.RESERVED),
                    group.Count(row => row.StatusId == ProductTableStatusIdConst.BLOCKED)));
    }

    private sealed record WarehouseMovementGroup(
        IReadOnlyList<InventoryMovementEntry> Entries,
        WarehouseProductMovement Movement,
        decimal? UnitCost);
    private sealed record WarehouseMovementGroupKey(
        int OrganizationId,
        int WarehouseId,
        int ProductId,
        short DocumentTypeId,
        long DocumentId,
        short OperationTypeId,
        long? SourceLineId,
        bool IsPieceTracked);
    private sealed record WarehouseProductSnapshot(int Id, short UnitId);
    private sealed record ProductTableProduct(int Id, int ProductId);
    private sealed record ProductTableBatchLink(int ProductTableId, long BatchId);
    private sealed record BatchReceivedDate(long BatchId, DateTime ReceivedDate);
    private sealed record DocumentNumber(long Id, string DocNumber);
    private sealed record WarehouseProductBatchCost(decimal RemainingQuantity, decimal? UnitCost);
    private sealed record OrganizationValuationMethod(int OrganizationId, string? InventoryValuationMethod);
    private sealed record WarehouseProductInitialBalanceRow(int WarehouseId, int ProductId, short StatusId);
    private sealed record WarehouseProductBalanceChange(
        int WarehouseId,
        int ProductId,
        short? UnitId,
        decimal QuantityDelta,
        decimal ReservedQuantityDelta,
        decimal BlockedQuantityDelta);

    private sealed record WarehouseProductInitialBalance(decimal Quantity, decimal ReservedQuantity, decimal BlockedQuantity);
}
