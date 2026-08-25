using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class PurchaseInventoryHandler : IInventoryDocumentHandler<PurchaseDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
    {
        var entries = new List<InventoryMovementEntry>();
        foreach (var line in purchase.PurchaseDocProducts.Where(line => !line.Product.IsService))
        {
            foreach (var table in line.PurchaseDocTables)
            {
                entries.Add(new InventoryMovementEntry
                {
                    OrganizationId = purchase.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                    DocumentId = purchase.Id,
                    WarehouseId = purchase.WarehouseId,
                    ProductId = line.ProductId,
                    ProductTableId = table.ProductTableId,
                    DirectionId = MovementDirectionIdConst.IN,
                    Quantity = 1m,
                    Amount = table.TotalAmount,
                    DocDate = purchase.DocDate,
                    SourceLineId = line.Id
                });
            }

            var quantityWithoutMarking = line.Quantity - line.PurchaseDocTables.Count;
            if (quantityWithoutMarking <= 0m)
                continue;

            entries.Add(new InventoryMovementEntry
            {
                OrganizationId = purchase.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.PURCHASE,
                DocumentId = purchase.Id,
                WarehouseId = purchase.WarehouseId,
                ProductId = line.ProductId,
                ProductTableId = null,
                DirectionId = MovementDirectionIdConst.IN,
                Quantity = quantityWithoutMarking,
                Amount = line.TotalAmount - line.PurchaseDocTables.Sum(table => table.TotalAmount),
                DocDate = purchase.DocDate,
                SourceLineId = line.Id
            });
        }

        return Task.FromResult(Result.Success(entries));
    }
}
