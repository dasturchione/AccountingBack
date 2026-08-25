using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class InventoryAdjustmentInventoryHandler : IInventoryDocumentHandler<InventoryAdjustmentDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(InventoryAdjustmentDoc document, CancellationToken ct = default)
    {
        var entries = new List<InventoryMovementEntry>();

        foreach (var line in document.InventoryAdjustmentLines.Where(line => !line.Product.IsService))
        {
            if (!line.Product.IsPieceTracked)
            {
                entries.Add(CreateEntry(document, line, document.DirectionId, null, line.Quantity, 0m, line.Id));
                continue;
            }

            foreach (var table in line.InventoryAdjustmentDocTables)
                entries.Add(CreateEntry(document, line, document.DirectionId, table.ProductTableId, 1m, table.CostPrice, table.Id));
        }

        return Task.FromResult(Result.Success(entries));
    }

    private static InventoryMovementEntry CreateEntry(
        InventoryAdjustmentDoc document,
        InventoryAdjustmentLine line,
        short directionId,
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
            DirectionId = directionId,
            Quantity = quantity,
            Amount = amount,
            DocDate = document.DocDate,
            SourceLineId = sourceLineId
        };
}
