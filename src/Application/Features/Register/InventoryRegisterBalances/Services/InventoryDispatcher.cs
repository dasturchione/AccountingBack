using Application.Abstractions;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryDispatcher : IInventoryDispatcher
{
    private readonly IInventoryDocumentHandler<PurchaseDoc> _purchaseHandler;
    private readonly IInventoryDocumentHandler<SaleDoc> _saleHandler;
    private readonly IInventoryDocumentHandler<WarehouseTransferDoc> _warehouseTransferHandler;
    private readonly IInventoryDocumentHandler<InventoryAdjustmentDoc> _inventoryAdjustmentHandler;
    private readonly ICommandRepository<RegisterBalance> _command;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;

    public InventoryDispatcher(IInventoryDocumentHandler<PurchaseDoc> purchaseHandler,
                               IInventoryDocumentHandler<SaleDoc> saleHandler,
                               IInventoryDocumentHandler<WarehouseTransferDoc> warehouseTransferHandler,
                               IInventoryDocumentHandler<InventoryAdjustmentDoc> inventoryAdjustmentHandler,
                               ICommandRepository<RegisterBalance> command,
                               IWarehouseProductBalanceService warehouseProductBalanceService)
    {
        _purchaseHandler = purchaseHandler;
        _saleHandler = saleHandler;
        _warehouseTransferHandler = warehouseTransferHandler;
        _inventoryAdjustmentHandler = inventoryAdjustmentHandler;
        _command = command;
        _warehouseProductBalanceService = warehouseProductBalanceService;
    }

    public async Task<Result<List<RegisterBalance>>> ProcessAsync(object document, CancellationToken ct = default, long? postingBatchId = null)
    {
        var result = document switch
        {
            PurchaseDoc p => await _purchaseHandler.HandleAsync(p, ct),
            SaleDoc s => await _saleHandler.HandleAsync(s, ct),
            WarehouseTransferDoc t => await _warehouseTransferHandler.HandleAsync(t, ct),
            InventoryAdjustmentDoc a => await _inventoryAdjustmentHandler.HandleAsync(a, ct),
            _ => Result.Failure<List<RegisterBalance>>(InventoryRegisterBalanceErrors.UnsupportedDocumentType())
        };

        if (!result.IsSuccess)
            return result;

        if (postingBatchId.HasValue)
        {
            foreach (var entry in result.Value)
                entry.PostingBatchId = postingBatchId.Value;
        }

        await _command.CreateAsync(result.Value, ct);
        var warehouseProductUpdate = await _warehouseProductBalanceService.ApplyInventoryEntriesAsync(result.Value, ct);
        if (!warehouseProductUpdate.IsSuccess)
            return Result.Failure<List<RegisterBalance>>(warehouseProductUpdate.Error);

        return result;
    }
}
