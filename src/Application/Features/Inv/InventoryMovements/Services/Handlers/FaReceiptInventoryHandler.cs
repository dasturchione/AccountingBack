using Application.Features.FaReceipts;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public sealed class FaReceiptInventoryHandler : IInventoryDocumentHandler<FaReceiptDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(
        FaReceiptDoc document,
        CancellationToken ct = default)
    {
        var entries = new List<InventoryMovementEntry>();

        foreach (var line in document.Lines)
        {
            var sourceProductId = line.SourceProductId;
            if (!sourceProductId.HasValue)
                continue;

            if (line.SourceProduct is null)
            {
                return Task.FromResult(Result.Failure<List<InventoryMovementEntry>>(
                    FaReceiptErrors.ProductNotFound(sourceProductId.Value)));
            }

            if (line.SourceProduct.IsService)
                continue;

            var warehouseId = document.WarehouseId;
            if (!warehouseId.HasValue)
            {
                return Task.FromResult(Result.Failure<List<InventoryMovementEntry>>(
                    FaReceiptErrors.WarehouseRequiredForSourceProduct()));
            }

            if (!line.SourceProduct.IsPieceTracked)
            {
                entries.Add(new InventoryMovementEntry
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
                    DocumentId = document.Id,
                    WarehouseId = warehouseId.Value,
                    ProductId = sourceProductId.Value,
                    OperationTypeId = OperationTypeIdConst.OUT,
                    Quantity = line.Quantity,
                    Amount = line.Amount,
                    DocDate = document.DocDate,
                    SourceLineId = line.Id
                });
                continue;
            }

            foreach (var receiptAsset in line.Assets)
            {
                var sourceProductTableId = receiptAsset.FaAsset?.SourceProductTableId;
                if (!sourceProductTableId.HasValue)
                {
                    return Task.FromResult(Result.Failure<List<InventoryMovementEntry>>(
                        FaReceiptErrors.SourceProductTableRequired(receiptAsset.Id)));
                }

                entries.Add(new InventoryMovementEntry
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.FARECEIPT,
                    DocumentId = document.Id,
                    WarehouseId = warehouseId.Value,
                    ProductId = sourceProductId.Value,
                    ProductTableId = sourceProductTableId.Value,
                    OperationTypeId = OperationTypeIdConst.OUT,
                    Quantity = 1m,
                    Amount = receiptAsset.InitialCost,
                    DocDate = document.DocDate,
                    SourceLineId = receiptAsset.Id
                });
            }
        }

        return Task.FromResult(Result.Success(entries));
    }
}