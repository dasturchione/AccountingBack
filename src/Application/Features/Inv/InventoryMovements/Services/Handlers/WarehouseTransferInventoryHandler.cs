using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public sealed class WarehouseTransferInventoryHandler : IInventoryDocumentHandler<WarehouseTransferDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(WarehouseTransferDoc document, CancellationToken ct = default)
    {
        var entries = new List<InventoryMovementEntry>();

        foreach (var line in document.WarehouseTransferLines.Where(line => !line.Product.IsService))
        {
            if (!line.Product.IsPieceTracked)
            {
                entries.Add(CreateEntry(document, line, document.SourceWarehouseId, MovementDirectionIdConst.OUT, null, line.Quantity, 0m, line.Id));
                entries.Add(CreateEntry(document, line, document.DestinationWarehouseId, MovementDirectionIdConst.IN, null, line.Quantity, 0m, line.Id));
                continue;
            }

            foreach (var table in line.WarehouseTransferDocTables)
            {
                entries.Add(CreateEntry(document, line, document.SourceWarehouseId, MovementDirectionIdConst.OUT, table.ProductTableId, 1m, table.CostPrice, table.Id));
                entries.Add(CreateEntry(document, line, document.DestinationWarehouseId, MovementDirectionIdConst.IN, table.ProductTableId, 1m, table.CostPrice, table.Id));
            }
        }

        return Task.FromResult(Result.Success(entries));
    }

    private static InventoryMovementEntry CreateEntry(
        WarehouseTransferDoc document,
        WarehouseTransferLine line,
        int warehouseId,
        short directionId,
        int? productTableId,
        decimal quantity,
        decimal amount,
        long sourceLineId) =>
        new()
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
            DocumentId = document.Id,
            WarehouseId = warehouseId,
            ProductId = line.ProductId,
            ProductTableId = productTableId,
            DirectionId = directionId,
            Quantity = quantity,
            Amount = amount,
            DocDate = document.DocDate,
            SourceLineId = sourceLineId
        };
}
