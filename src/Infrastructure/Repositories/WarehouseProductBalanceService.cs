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

        var entryList = entries.ToList();
        var operationValidation = ValidateOperations(entryList);
        if (!operationValidation.IsSuccess)
            return operationValidation;

        var products = await GetProductSnapshotsAsync(entryList.Select(x => x.ProductId), ct);
        if (products.Count != entryList.Select(x => x.ProductId).Distinct().Count())
            return Result.Failure(WarehouseProductErrors.ProductNotFound(
                entryList.Select(x => x.ProductId).First(x => !products.ContainsKey(x)),
                _userContext.LanguageId));

        var productUnitIds = products.ToDictionary(x => x.Key, x => x.Value.UnitId);
        var movementResult = await ApplyMovementsAndBatchesAsync(entryList, products, ct);
        if (!movementResult.IsSuccess)
            return movementResult;

        var productTableEntries = entryList.Where(x => x.ProductTableId.HasValue).ToList();
        var productTablesResult = await GetProductTablesAsync(productTableEntries, ct);
        if (!productTablesResult.IsSuccess)
            return Result.Failure(productTablesResult.Error);

        var productTablesById = productTablesResult.Value;
        var warehouseProductTables = await GetWarehouseProductTablesAsync(productTableEntries, ct);
        var warehouseProductTablesById = warehouseProductTables.ToDictionary(x => x.ProductTableId);
        var changes = new List<WarehouseProductBalanceChange>();
        var processedEntries = new HashSet<RegisterBalance>();

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

            changes.Add(ToQuantityChange(sourceEntry));
            changes.Add(ToQuantityChange(destinationEntry));
            processedEntries.Add(sourceEntry);
            processedEntries.Add(destinationEntry);
        }

        var receivedDates = await GetReceivedDatesAsync(
            productTableEntries
                .Where(x => x.OperationTypeId == OperationTypeIdConst.IN && x.ReversalEntryId.HasValue)
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

                await _context.Set<WarehouseProductTable>().AddAsync(warehouseProductTable, ct);
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

            changes.Add(ToQuantityChange(
                entry,
                existingWarehouseProductTable.StatusId == ProductTableStatusIdConst.RESERVED ? -entry.Quantity : 0m));
            _context.Set<WarehouseProductTable>().Remove(existingWarehouseProductTable);
            warehouseProductTablesById.Remove(productTableId);
        }

        var balanceResult = await ApplyWarehouseProductChangesAsync(changes, productUnitIds, ct);
        if (!balanceResult.IsSuccess)
            return balanceResult;

        await _context.SaveChangesAsync(ct);
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
        if (distinctProductTableIds.Count != productTableIds.Count)
            return Result.Failure(WarehouseProductErrors.ProductTableUnavailable(productTableIds.First(), 0, _userContext.LanguageId));

        if (distinctProductTableIds.Count > 0)
        {
            var tables = await _context.Set<WarehouseProductTable>()
                .Include(x => x.ProductTable)
                .Where(x => distinctProductTableIds.Contains(x.ProductTableId))
                .ToListAsync(ct);

            if (tables.Count != distinctProductTableIds.Count)
            {
                var foundIds = tables.Select(x => x.ProductTableId).ToHashSet();
                return Result.Failure(WarehouseProductErrors.ProductTableNotFound(
                    distinctProductTableIds.First(x => !foundIds.Contains(x)),
                    _userContext.LanguageId));
            }

            var selectedByProduct = tables
                .GroupBy(x => x.ProductTable.ProductId)
                .ToDictionary(x => x.Key, x => (decimal)x.Count());
            var requestedByProduct = items
                .GroupBy(x => x.ProductId)
                .ToDictionary(x => x.Key, x => x.Sum(item => item.Quantity));

            foreach (var selected in selectedByProduct)
            {
                if (!requestedByProduct.TryGetValue(selected.Key, out var requested) || requested < selected.Value)
                    return Result.Failure(WarehouseProductErrors.NotEnoughQuantity(
                        warehouseId,
                        selected.Key,
                        selected.Value,
                        requested,
                        _userContext.LanguageId));
            }

            var expectedStatus = reserve ? ProductTableStatusIdConst.IN_STOCK : ProductTableStatusIdConst.RESERVED;
            var nextStatus = reserve ? ProductTableStatusIdConst.RESERVED : ProductTableStatusIdConst.IN_STOCK;

            foreach (var table in tables)
            {
                var validation = ValidateWarehouseProductTable(
                    table,
                    table.ProductTableId,
                    table.ProductTable.ProductId,
                    warehouseId,
                    expectedStatus);
                if (!validation.IsSuccess)
                    return validation;

                table.StatusId = nextStatus;
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

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Result ValidateOperations(IReadOnlyCollection<RegisterBalance> entries)
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
        IReadOnlyCollection<RegisterBalance> entries,
        IReadOnlyDictionary<int, WarehouseProductSnapshot> products,
        CancellationToken ct)
    {
        var now = DateTime.Now;
        var movementsByEntry = entries.ToDictionary(
            entry => entry,
            entry => new WarehouseProductMovement
            {
                OrganizationId = entry.OrganizationId,
                WarehouseId = entry.WarehouseId,
                ProductId = entry.ProductId,
                DocumentTypeId = entry.DocumentTypeId,
                DocumentId = entry.DocumentId,
                DocumentLineId = entry.SourceLineId,
                Quantity = entry.Quantity,
                MovementSign = ToMovementSign(entry.OperationTypeId),
                MovementDate = entry.DocDate,
                CreatedDate = now
            });

        await _context.Set<WarehouseProductMovement>().AddRangeAsync(movementsByEntry.Values, ct);

        var bulkEntries = entries
            .Where(entry => !entry.ProductTableId.HasValue && !products[entry.ProductId].IsPieceTracked)
            .ToList();
        if (bulkEntries.Count == 0)
            return Result.Success();

        var valuationMethods = await GetInventoryValuationMethodsAsync(
            bulkEntries.Select(entry => entry.OrganizationId),
            ct);
        var originalEntries = await GetOriginalEntriesAsync(
            bulkEntries.Where(entry => entry.ReversalEntryId.HasValue)
                .Select(entry => entry.ReversalEntryId!.Value),
            ct);
        var processedEntries = new HashSet<RegisterBalance>();

        foreach (var transferGroup in GetBulkTransferGroups(bulkEntries))
        {
            var issueEntry = transferGroup.Single(entry => entry.OperationTypeId == OperationTypeIdConst.OUT);
            var receiptEntry = transferGroup.Single(entry => entry.OperationTypeId == OperationTypeIdConst.IN);

            var issueResult = await ApplyIssueMovementAsync(
                issueEntry,
                movementsByEntry[issueEntry],
                valuationMethods[issueEntry.OrganizationId],
                originalEntries,
                ct);
            if (!issueResult.IsSuccess)
                return issueResult;

            receiptEntry.Amount = issueEntry.Amount;
            var receiptResult = await ApplyReceiptMovementAsync(
                receiptEntry,
                movementsByEntry[receiptEntry],
                originalEntries,
                ct);
            if (!receiptResult.IsSuccess)
                return receiptResult;

            processedEntries.Add(issueEntry);
            processedEntries.Add(receiptEntry);
        }

        foreach (var entry in bulkEntries.Where(entry => !processedEntries.Contains(entry)))
        {
            var movement = movementsByEntry[entry];
            var result = entry.OperationTypeId == OperationTypeIdConst.OUT
                ? await ApplyIssueMovementAsync(entry, movement, valuationMethods[entry.OrganizationId], originalEntries, ct)
                : await ApplyReceiptMovementAsync(entry, movement, originalEntries, ct);
            if (!result.IsSuccess)
                return result;
        }

        return Result.Success();
    }

    private async Task<Result> ApplyIssueMovementAsync(
        RegisterBalance entry,
        WarehouseProductMovement issueMovement,
        string valuationMethod,
        IReadOnlyDictionary<long, RegisterBalance> originalEntries,
        CancellationToken ct)
    {
        if (entry.ReversalEntryId.HasValue)
            return await ApplyReceiptReversalAsync(entry, issueMovement, originalEntries, ct);

        var batches = await GetAvailableBatchesAsync(
            entry.OrganizationId,
            entry.WarehouseId,
            entry.ProductId,
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
            AddBatchAllocation(issueMovement, batch, quantity, unitCost);
            totalCost += quantity * unitCost;
            quantityToAllocate -= quantity;
        }

        entry.Amount = totalCost;
        return Result.Success();
    }

    private async Task<Result> ApplyReceiptMovementAsync(
        RegisterBalance entry,
        WarehouseProductMovement receiptMovement,
        IReadOnlyDictionary<long, RegisterBalance> originalEntries,
        CancellationToken ct)
    {
        if (entry.ReversalEntryId.HasValue)
            return await ApplyIssueReversalAsync(entry, originalEntries, ct);

        var unitCost = entry.Quantity == 0m ? 0m : entry.Amount / entry.Quantity;
        if (unitCost == 0m && entry.DocumentTypeId == DocumentTypeIdConst.INVENTORYADJUSTMENT)
        {
            unitCost = await GetWeightedUnitCostAsync(entry.OrganizationId, entry.WarehouseId, entry.ProductId, ct);
            entry.Amount = unitCost * entry.Quantity;
        }

        var batch = new WarehouseProductBatch
        {
            OrganizationId = entry.OrganizationId,
            WarehouseId = entry.WarehouseId,
            ProductId = entry.ProductId,
            ReceiptMovement = receiptMovement,
            InitialQuantity = entry.Quantity,
            RemainingQuantity = entry.Quantity,
            UnitCost = unitCost,
            ReceivedDate = entry.DocDate,
            CreatedDate = DateTime.Now
        };

        await _context.Set<WarehouseProductBatch>().AddAsync(batch, ct);
        return Result.Success();
    }

    private async Task<Result> ApplyReceiptReversalAsync(
        RegisterBalance reversalEntry,
        WarehouseProductMovement issueMovement,
        IReadOnlyDictionary<long, RegisterBalance> originalEntries,
        CancellationToken ct)
    {
        if (!originalEntries.TryGetValue(reversalEntry.ReversalEntryId!.Value, out var originalEntry))
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(reversalEntry.ReversalEntryId.Value, _userContext.LanguageId));

        var originalMovement = await FindMovementAsync(originalEntry, ct);
        if (originalMovement == null)
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(originalEntry.Id, _userContext.LanguageId));

        var batch = await _context.Set<WarehouseProductBatch>()
            .SingleOrDefaultAsync(item => item.ReceiptMovementId == originalMovement.Id, ct);
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
        AddBatchAllocation(issueMovement, batch, reversalEntry.Quantity, unitCost);
        reversalEntry.Amount = reversalEntry.Quantity * unitCost;
        return Result.Success();
    }

    private async Task<Result> ApplyIssueReversalAsync(
        RegisterBalance reversalEntry,
        IReadOnlyDictionary<long, RegisterBalance> originalEntries,
        CancellationToken ct)
    {
        if (!originalEntries.TryGetValue(reversalEntry.ReversalEntryId!.Value, out var originalEntry))
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(reversalEntry.ReversalEntryId.Value, _userContext.LanguageId));

        var originalMovement = await FindMovementAsync(originalEntry, ct);
        if (originalMovement == null)
            return Result.Failure(WarehouseProductErrors.OriginalMovementNotFound(originalEntry.Id, _userContext.LanguageId));

        var allocations = await _context.Set<WarehouseProductBatchAllocation>()
            .Include(item => item.Batch)
            .Where(item => item.IssueMovementId == originalMovement.Id)
            .ToListAsync(ct);
        if (allocations.Count == 0)
            return Result.Failure(WarehouseProductErrors.OriginalAllocationNotFound(originalMovement.Id, _userContext.LanguageId));

        var allocatedQuantity = allocations.Sum(item => item.Quantity);
        if (allocatedQuantity != reversalEntry.Quantity)
            return Result.Failure(WarehouseProductErrors.OriginalAllocationNotFound(originalMovement.Id, _userContext.LanguageId));

        foreach (var allocation in allocations)
            allocation.Batch.RemainingQuantity += allocation.Quantity;

        reversalEntry.Amount = allocations.Sum(item => item.Quantity * (item.UnitCost ?? 0m));
        return Result.Success();
    }

    private async Task<WarehouseProductMovement?> FindMovementAsync(RegisterBalance entry, CancellationToken ct) =>
        await _context.Set<WarehouseProductMovement>()
            .Where(item => item.OrganizationId == entry.OrganizationId &&
                           item.WarehouseId == entry.WarehouseId &&
                           item.ProductId == entry.ProductId &&
                           item.DocumentTypeId == entry.DocumentTypeId &&
                           item.DocumentId == entry.DocumentId &&
                           item.DocumentLineId == entry.SourceLineId &&
                           item.MovementSign == ToMovementSign(entry.OperationTypeId))
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(ct);

    private async Task<List<WarehouseProductBatch>> GetAvailableBatchesAsync(
        int organizationId,
        int warehouseId,
        int productId,
        bool lifo,
        CancellationToken ct)
    {
        var query = _context.Set<WarehouseProductBatch>()
            .Where(item => item.OrganizationId == organizationId &&
                           item.WarehouseId == warehouseId &&
                           item.ProductId == productId &&
                           item.RemainingQuantity > 0m);

        return lifo
            ? await query.OrderByDescending(item => item.ReceivedDate).ThenByDescending(item => item.Id).ToListAsync(ct)
            : await query.OrderBy(item => item.ReceivedDate).ThenBy(item => item.Id).ToListAsync(ct);
    }

    private async Task<decimal> GetWeightedUnitCostAsync(int organizationId, int warehouseId, int productId, CancellationToken ct)
    {
        var batches = await _context.Set<WarehouseProductBatch>()
            .Where(item => item.OrganizationId == organizationId &&
                           item.WarehouseId == warehouseId &&
                           item.ProductId == productId &&
                           item.RemainingQuantity > 0m)
            .Select(item => new { item.RemainingQuantity, item.UnitCost })
            .ToListAsync(ct);

        var quantity = batches.Sum(item => item.RemainingQuantity);
        return quantity == 0m
            ? 0m
            : batches.Sum(item => item.RemainingQuantity * (item.UnitCost ?? 0m)) / quantity;
    }

    private async Task<Dictionary<int, string>> GetInventoryValuationMethodsAsync(IEnumerable<int> organizationIds, CancellationToken ct)
    {
        var ids = organizationIds.Distinct().ToList();
        var configured = await _context.Set<OrganizationConfig>()
            .Where(config => ids.Contains(config.OrganizationId))
            .Select(config => new { config.OrganizationId, config.InventoryValuationMethod })
            .ToDictionaryAsync(config => config.OrganizationId, config => config.InventoryValuationMethod, ct);

        return ids.ToDictionary(
            id => id,
            id => NormalizeInventoryValuationMethod(configured.GetValueOrDefault(id)));
    }

    private async Task<Dictionary<long, RegisterBalance>> GetOriginalEntriesAsync(IEnumerable<long> entryIds, CancellationToken ct)
    {
        var ids = entryIds.Distinct().ToList();
        return ids.Count == 0
            ? new Dictionary<long, RegisterBalance>()
            : await _context.Set<RegisterBalance>()
                .Where(entry => ids.Contains(entry.Id))
                .ToDictionaryAsync(entry => entry.Id, ct);
    }

    private void AddBatchAllocation(
        WarehouseProductMovement issueMovement,
        WarehouseProductBatch batch,
        decimal quantity,
        decimal unitCost)
    {
        _context.Set<WarehouseProductBatchAllocation>().Add(new WarehouseProductBatchAllocation
        {
            IssueMovement = issueMovement,
            Batch = batch,
            Quantity = quantity,
            UnitCost = unitCost,
            CreatedDate = DateTime.Now
        });
    }

    private static IEnumerable<IGrouping<string, RegisterBalance>> GetBulkTransferGroups(IEnumerable<RegisterBalance> entries) =>
        entries
            .Where(entry => entry.DocumentTypeId == DocumentTypeIdConst.WAREHOUSETRANSFER)
            .GroupBy(entry => $"{entry.DocumentId}:{entry.SourceLineId}:{entry.ProductId}")
            .Where(group => group.Count() == 2 &&
                            group.Count(entry => entry.OperationTypeId == OperationTypeIdConst.IN) == 1 &&
                            group.Count(entry => entry.OperationTypeId == OperationTypeIdConst.OUT) == 1);

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

        return await _context.Set<Product>()
            .Where(product => ids.Contains(product.Id))
            .Select(product => new WarehouseProductSnapshot(product.Id, product.UnitId, product.IsPieceTracked))
            .ToDictionaryAsync(product => product.Id, ct);
    }
    private async Task<Result<Dictionary<int, int>>> GetProductTablesAsync(
        IReadOnlyCollection<RegisterBalance> entries,
        CancellationToken ct)
    {
        var productTableIds = entries.Select(x => x.ProductTableId!.Value).Distinct().ToList();
        if (productTableIds.Count == 0)
            return Result.Success(new Dictionary<int, int>());

        var rows = await _context.Set<ProductTable>()
            .Where(x => productTableIds.Contains(x.Id))
            .Select(x => new { x.Id, x.ProductId })
            .ToListAsync(ct);

        var byId = rows.ToDictionary(x => x.Id, x => x.ProductId);
        var missingId = productTableIds.FirstOrDefault(x => !byId.ContainsKey(x));
        return missingId == 0
            ? Result.Success(byId)
            : Result.Failure<Dictionary<int, int>>(WarehouseProductErrors.ProductTableNotFound(missingId, _userContext.LanguageId));
    }

    private Task<List<WarehouseProductTable>> GetWarehouseProductTablesAsync(
        IReadOnlyCollection<RegisterBalance> entries,
        CancellationToken ct)
    {
        var productTableIds = entries.Select(x => x.ProductTableId!.Value).Distinct().ToList();
        return productTableIds.Count == 0
            ? Task.FromResult(new List<WarehouseProductTable>())
            : _context.Set<WarehouseProductTable>()
                .Where(x => productTableIds.Contains(x.ProductTableId))
                .ToListAsync(ct);
    }

    private async Task<Dictionary<int, DateTime>> GetReceivedDatesAsync(IEnumerable<int> productTableIds, CancellationToken ct)
    {
        var ids = productTableIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, DateTime>();

        var rows = await _context.Set<RegisterBalance>()
            .Where(x => x.ProductTableId.HasValue &&
                        ids.Contains(x.ProductTableId.Value) &&
                        x.OperationTypeId == OperationTypeIdConst.IN &&
                        x.ReversalEntryId == null)
            .GroupBy(x => x.ProductTableId!.Value)
            .Select(x => new { ProductTableId = x.Key, ReceivedDate = x.Min(entry => entry.DocDate) })
            .ToListAsync(ct);

        return rows.ToDictionary(x => x.ProductTableId, x => x.ReceivedDate);
    }

    private static IEnumerable<IGrouping<int, RegisterBalance>> GetTransferGroups(IReadOnlyCollection<RegisterBalance> entries) =>
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

    private static WarehouseProductBalanceChange ToQuantityChange(RegisterBalance entry, decimal reservedQuantityDelta = 0m) =>
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

        return await _context.Set<Product>()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.UnitId })
            .ToDictionaryAsync(x => x.Id, x => x.UnitId, ct);
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
        var existing = await _context.Set<WarehouseProduct>()
            .Where(x => warehouseIds.Contains(x.WarehouseId) && productIds.Contains(x.ProductId))
            .ToListAsync(ct);
        var existingByKey = existing.ToDictionary(x => (x.WarehouseId, x.ProductId));

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

                await _context.Set<WarehouseProduct>().AddAsync(warehouseProduct, ct);
                existingByKey[key] = warehouseProduct;
            }

            var validation = ValidateChange(warehouseProduct, change);
            if (!validation.IsSuccess)
                return validation;

            warehouseProduct.Quantity += change.QuantityDelta;
            warehouseProduct.ReservedQuantity += change.ReservedQuantityDelta;
            warehouseProduct.BlockedQuantity += change.BlockedQuantityDelta;
            warehouseProduct.AvailableQuantity = warehouseProduct.Quantity - warehouseProduct.ReservedQuantity - warehouseProduct.BlockedQuantity;
        }

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
        var rows = await _context.Set<WarehouseProductTable>()
            .Where(x => warehouseIds.Contains(x.WarehouseId) &&
                        productIds.Contains(x.ProductTable.ProductId) &&
                        (x.StatusId == ProductTableStatusIdConst.IN_STOCK ||
                         x.StatusId == ProductTableStatusIdConst.RESERVED ||
                         x.StatusId == ProductTableStatusIdConst.BLOCKED))
            .GroupBy(x => new { x.WarehouseId, ProductId = x.ProductTable.ProductId, x.StatusId })
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
                    x.Sum(row => (decimal)row.Quantity),
                    x.Where(row => row.StatusId == ProductTableStatusIdConst.RESERVED).Sum(row => (decimal)row.Quantity),
                    x.Where(row => row.StatusId == ProductTableStatusIdConst.BLOCKED).Sum(row => (decimal)row.Quantity)));
    }

    private sealed record WarehouseProductSnapshot(int Id, short UnitId, bool IsPieceTracked);
    private sealed record WarehouseProductBalanceChange(
        int WarehouseId,
        int ProductId,
        short? UnitId,
        decimal QuantityDelta,
        decimal ReservedQuantityDelta,
        decimal BlockedQuantityDelta);

    private sealed record WarehouseProductInitialBalance(decimal Quantity, decimal ReservedQuantity, decimal BlockedQuantity);
}