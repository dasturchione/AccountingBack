using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryMovements;

public class SaleInventoryHandler : IInventoryDocumentHandler<SaleDoc>
{
    public Task<Result<List<InventoryMovementEntry>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
    {
        var entries = sale.SaleDocProducts
            .Where(p => !p.Product.IsService)
            .Select(line => new InventoryMovementEntry
            {
                OrganizationId  = sale.OrganizationId,
                DocumentTypeId  = DocumentTypeIdConst.SALE,
                DocumentId      = sale.Id,
                WarehouseId     = sale.WarehouseId,
                ProductId       = line.ProductId,
                ProductTableId  = null,
                DirectionId     = MovementDirectionIdConst.OUT,
                Quantity        = line.Quantity,
                Amount          = line.CostPrice * line.Quantity,
                DocDate         = sale.DocDate,
                SourceLineId    = line.Id
            });

        return Task.FromResult(Result.Success(entries.ToList()));
    }
}
