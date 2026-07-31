using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class PurchaseInventoryHandler : IInventoryDocumentHandler<PurchaseDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
    {
        var trackedEntries = purchase.PurchaseDocProducts
            .Where(line => !line.Product.IsService && line.Product.IsPieceTracked)
            .SelectMany(line => line.PurchaseDocTables.Select(table => new InventoryMovementEntry
            {
                OrganizationId = purchase.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentId = purchase.Id,
                WarehouseId = purchase.WarehouseId,
                ProductId = line.ProductId,
                ProductTableId = table.ProductTableId,
                OperationTypeId = OperationTypeIdConst.IN,
                Quantity = 1m,
                Amount = table.TotalAmount,
                DocDate = purchase.DocDate,
                SourceLineId = table.Id
            }));

        var nonTrackedEntries = purchase.PurchaseDocProducts
            .Where(line => !line.Product.IsService && !line.Product.IsPieceTracked)
            .Select(line => new InventoryMovementEntry
            {
                OrganizationId = purchase.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentId = purchase.Id,
                WarehouseId = purchase.WarehouseId,
                ProductId = line.ProductId,
                ProductTableId = null,
                OperationTypeId = OperationTypeIdConst.IN,
                Quantity = line.Quantity,
                Amount = line.TotalAmount,
                DocDate = purchase.DocDate,
                SourceLineId = line.Id
            });

        return Task.FromResult(Result.Success(trackedEntries.Concat(nonTrackedEntries).ToList()));
    }
}
