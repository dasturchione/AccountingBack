using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public sealed class OpeningInventoryHandler : IInventoryDocumentHandler<OpeningInventory>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(
        OpeningInventory document,
        CancellationToken ct = default)
    {
        var tracked = document.OpeningInventoryProducts
            .Where(line => !line.Product.IsService && line.Product.IsPieceTracked)
            .SelectMany(line => line.OpeningInventoryTables.Select(item => new InventoryMovementEntry
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.OPENINGINVENTORY,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = line.ProductId,
                ProductTableId = item.ProductTableId,
                DirectionId = MovementDirectionIdConst.IN,
                Quantity = 1m,
                Amount = item.Amount,
                DocDate = document.DocDate,
                SourceLineId = item.Id
            }));

        var nonTracked = document.OpeningInventoryProducts
            .Where(line => !line.Product.IsService && !line.Product.IsPieceTracked)
            .Select(line => new InventoryMovementEntry
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.OPENINGINVENTORY,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = line.ProductId,
                DirectionId = MovementDirectionIdConst.IN,
                Quantity = line.Quantity,
                Amount = line.Amount,
                DocDate = document.DocDate,
                SourceLineId = line.Id
            });

        return Task.FromResult(Result.Success(tracked.Concat(nonTracked).ToList()));
    }
}
