using Application.Abstractions;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class InventoryDispatcher : IInventoryDispatcher
{
    private readonly IInventoryDocumentHandler<PurchaseDoc> _purchaseHandler;
    private readonly IInventoryDocumentHandler<SaleDoc> _saleHandler;
    private readonly IInventoryDocumentHandler<RetailSaleDoc> _retailSaleHandler;
    private readonly IInventoryDocumentHandler<WarehouseTransferDoc> _warehouseTransferHandler;
    private readonly IInventoryDocumentHandler<InventoryAdjustmentDoc> _inventoryAdjustmentHandler;
    private readonly IInventoryDocumentHandler<OpeningInventory> _openingInventoryHandler;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<WarehouseProductMovement> _movementQuery;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;

    public InventoryDispatcher(IInventoryDocumentHandler<PurchaseDoc> purchaseHandler,
                               IInventoryDocumentHandler<SaleDoc> saleHandler,
                               IInventoryDocumentHandler<RetailSaleDoc> retailSaleHandler,
                               IInventoryDocumentHandler<WarehouseTransferDoc> warehouseTransferHandler,
                               IInventoryDocumentHandler<InventoryAdjustmentDoc> inventoryAdjustmentHandler,
                               IInventoryDocumentHandler<OpeningInventory> openingInventoryHandler,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<WarehouseProductMovement> movementQuery,
                               IWarehouseProductBalanceService warehouseProductBalanceService)
    {
        _purchaseHandler = purchaseHandler;
        _saleHandler = saleHandler;
        _retailSaleHandler = retailSaleHandler;
        _warehouseTransferHandler = warehouseTransferHandler;
        _inventoryAdjustmentHandler = inventoryAdjustmentHandler;
        _openingInventoryHandler = openingInventoryHandler;
        _queryBuilder = queryBuilder;
        _movementQuery = movementQuery;
        _warehouseProductBalanceService = warehouseProductBalanceService;
    }

    public async Task<Result<List<InventoryMovementEntry>>> ProcessAsync(
        object document,
        CancellationToken ct = default,
        long? postingBatchId = null)
    {
        var result = await BuildEntriesAsync(document, ct);

        if (!result.IsSuccess)
            return result;

        var warehouseProductUpdate = document switch
        {
            SaleDoc sale => await _warehouseProductBalanceService.ApplySaleInventoryEntriesAsync(sale, result.Value, ct),
            RetailSaleDoc retailSale => await _warehouseProductBalanceService.ApplyRetailSaleInventoryEntriesAsync(retailSale, result.Value, ct),
            _ => await _warehouseProductBalanceService.ApplyInventoryEntriesAsync(result.Value, ct)
        };
        if (!warehouseProductUpdate.IsSuccess)
            return Result.Failure<List<InventoryMovementEntry>>(warehouseProductUpdate.Error);

        return result;
    }

    public async Task<Result> ReverseAsync(object document, CancellationToken ct = default)
    {
        var entriesResult = await BuildEntriesAsync(document, ct);
        if (!entriesResult.IsSuccess)
            return Result.Failure(entriesResult.Error);

        var entries = entriesResult.Value;
        if (entries.Count == 0)
            return Result.Success();

        var first = entries[0];
        var movements = await _movementQuery.GetAllAsync(
            _queryBuilder.For<WarehouseProductMovement>()
                .Where(x => x.OrganizationId == first.OrganizationId &&
                            x.DocumentTypeId == first.DocumentTypeId &&
                            x.DocumentId == first.DocumentId)
                .Build(),
            ct);

        var movementsByKey = movements
            .GroupBy(x => new MovementKey(
                x.OrganizationId,
                x.WarehouseId,
                x.ProductId,
                x.DocumentTypeId,
                x.DocumentId,
                x.DirectionId,
                x.DocumentLineId))
            .ToDictionary(x => x.Key, x => x.OrderBy(movement => movement.Id).First());

        foreach (var group in entries.GroupBy(ToMovementKey))
        {
            if (!movementsByKey.TryGetValue(group.Key, out var originalMovement) ||
                originalMovement.Quantity != group.Sum(entry => entry.Quantity))
            {
                return Result.Failure(
                    InventoryMovementErrors.OriginalMovementsNotFound(first.DocumentTypeId, first.DocumentId));
            }

            foreach (var entry in group)
            {
                entry.OriginalMovementId = originalMovement.Id;
                entry.DirectionId = MovementDirectionIdConst.Reverse(entry.DirectionId);
                entry.DocDate = DateTime.Now;
            }
        }

        return document switch
        {
            SaleDoc sale => await _warehouseProductBalanceService.ReverseSaleInventoryEntriesAsync(sale, entries, ct),
            RetailSaleDoc retailSale => await _warehouseProductBalanceService.ReverseRetailSaleInventoryEntriesAsync(retailSale, entries, ct),
            _ => await _warehouseProductBalanceService.ApplyInventoryEntriesAsync(entries, ct)
        };
    }

    private Task<Result<List<InventoryMovementEntry>>> BuildEntriesAsync(object document, CancellationToken ct) =>
        document switch
        {
            PurchaseDoc purchase => _purchaseHandler.HandleAsync(purchase, ct),
            SaleDoc sale => _saleHandler.HandleAsync(sale, ct),
            RetailSaleDoc retailSale => _retailSaleHandler.HandleAsync(retailSale, ct),
            WarehouseTransferDoc transfer => _warehouseTransferHandler.HandleAsync(transfer, ct),
            InventoryAdjustmentDoc adjustment => _inventoryAdjustmentHandler.HandleAsync(adjustment, ct),
            OpeningInventory openingInventory => _openingInventoryHandler.HandleAsync(openingInventory, ct),
            _ => Task.FromResult(Result.Failure<List<InventoryMovementEntry>>(InventoryMovementErrors.UnsupportedDocumentType()))
        };

    private static MovementKey ToMovementKey(InventoryMovementEntry entry) =>
        new(
            entry.OrganizationId,
            entry.WarehouseId,
            entry.ProductId,
            entry.DocumentTypeId,
            entry.DocumentId,
            entry.DirectionId,
            entry.SourceLineId);

    private sealed record MovementKey(
        int OrganizationId,
        int WarehouseId,
        int ProductId,
        short DocumentTypeId,
        long DocumentId,
        short DirectionId,
        long? DocumentLineId);
}
