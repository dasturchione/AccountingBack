using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class PurchaseInventoryHandler : IInventoryDocumentHandler<PurchaseDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
    {
        var trackedEntries = purchase.PurchaseDocProducts
            .Where(line => !line.Product.IsService && line.Product.IsPieceTracked)
            .SelectMany(line => line.PurchaseDocTables.Select(table => new RegisterBalance
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
                CreatedDate = DateTime.Now,
                SourceLineId = table.Id
            }));

        var nonTrackedEntries = purchase.PurchaseDocProducts
            .Where(line => !line.Product.IsService && !line.Product.IsPieceTracked)
            .Select(line => new RegisterBalance
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
                CreatedDate = DateTime.Now,
                SourceLineId = line.Id
            });

        return Task.FromResult(Result.Success(trackedEntries.Concat(nonTrackedEntries).ToList()));
    }
}