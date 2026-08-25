using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public sealed class RetailSaleInventoryHandler : IInventoryDocumentHandler<RetailSaleDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(RetailSaleDoc document, CancellationToken ct = default)
    {
        var entries = document.RetailSaleDocProducts
            .Where(x => !x.Product.IsService)
            .Select(x => new InventoryMovementEntry
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.RETAIL_SALE,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = x.ProductId,
                DirectionId = MovementDirectionIdConst.OUT,
                Quantity = x.Quantity,
                Amount = x.CostPrice * x.Quantity,
                DocDate = document.DocDate,
                SourceLineId = x.Id
            });

        return Task.FromResult(Result.Success(entries.ToList()));
    }
}
