using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class InventoryAdjustmentInventoryHandler : IInventoryDocumentHandler<InventoryAdjustmentDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(InventoryAdjustmentDoc document, CancellationToken ct = default)
    {
        var operationTypeId = IsPositiveFlow(document.AdjustmentType)
            ? OperationTypeIdConst.IN
            : OperationTypeIdConst.OUT;
        var entries = new List<InventoryMovementEntry>();

        foreach (var line in document.InventoryAdjustmentLines.Where(line => !line.Product.IsService))
        {
            if (!line.Product.IsPieceTracked)
            {
                entries.Add(CreateEntry(document, line, operationTypeId, null, line.Quantity, 0m, line.Id));
                continue;
            }

            foreach (var table in line.InventoryAdjustmentDocTables)
                entries.Add(CreateEntry(document, line, operationTypeId, table.ProductTableId, 1m, table.CostPrice, table.Id));
        }

        return Task.FromResult(Result.Success(entries));
    }

    private static InventoryMovementEntry CreateEntry(
        InventoryAdjustmentDoc document,
        InventoryAdjustmentLine line,
        short operationTypeId,
        int? productTableId,
        decimal quantity,
        decimal amount,
        long sourceLineId) =>
        new()
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.INVENTORYADJUSTMENT,
            DocumentId = document.Id,
            WarehouseId = document.WarehouseId,
            ProductId = line.ProductId,
            ProductTableId = productTableId,
            OperationTypeId = operationTypeId,
            Quantity = quantity,
            Amount = amount,
            DocDate = document.DocDate,
            SourceLineId = sourceLineId
        };

    private static bool IsPositiveFlow(string adjustmentType) =>
        adjustmentType is "POSITIVE_ADJUSTMENT" or "FOUND_STOCK" or "CORRECTION";
}
