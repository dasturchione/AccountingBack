using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public sealed class RetailSaleInventoryHandler : IInventoryDocumentHandler<RetailSaleDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(RetailSaleDoc document, CancellationToken ct = default)
    {
        var pieceTracked = document.RetailSaleDocProducts
            .Where(x => !x.Product.IsService && x.Product.IsPieceTracked)
            .SelectMany(x => x.RetailSaleDocTables)
            .Select(x => new InventoryMovementEntry
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.RETAIL_SALE,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = x.ProductTable.ProductId,
                ProductTableId = x.ProductTableId,
                OperationTypeId = OperationTypeIdConst.OUT,
                Quantity = 1m,
                Amount = x.CostPrice,
                DocDate = document.DocDate,
                SourceLineId = x.Id
            });

        var nonPieceTracked = document.RetailSaleDocProducts
            .Where(x => !x.Product.IsService && !x.Product.IsPieceTracked)
            .Select(x => new InventoryMovementEntry
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.RETAIL_SALE,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = x.ProductId,
                OperationTypeId = OperationTypeIdConst.OUT,
                Quantity = x.Quantity,
                Amount = x.CostPrice * x.Quantity,
                DocDate = document.DocDate,
                SourceLineId = x.Id
            });

        return Task.FromResult(Result.Success(pieceTracked.Concat(nonPieceTracked).ToList()));
    }
}
