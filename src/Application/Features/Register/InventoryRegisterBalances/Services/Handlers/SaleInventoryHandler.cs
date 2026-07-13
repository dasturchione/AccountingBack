using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class SaleInventoryHandler : IInventoryDocumentHandler<SaleDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
    {
        var trackedEntries = sale.SaleDocProducts
            .Where(p => !p.Product.IsService && p.Product.IsPieceTracked)
            .SelectMany(p => p.SaleDocTables)
            .Select(line => new RegisterBalance
            {
                OrganizationId  = sale.OrganizationId,
                DocumentTypeId  = DocumentTypeIdConst.SALE,
                DocumentId      = sale.Id,
                WarehouseId     = sale.WarehouseId,
                ProductId       = line.ProductTable.ProductId,
                ProductTableId  = line.ProductTableId,
                OperationTypeId = OperationTypeIdConst.OUT,
                Quantity        = 1,
                Amount          = line.CostPrice,
                DocDate         = sale.DocDate,
                CreatedDate     = DateTime.Now,
                SourceLineId    = line.Id
            });

        var nonTrackedEntries = sale.SaleDocProducts
            .Where(p => !p.Product.IsService && !p.Product.IsPieceTracked)
            .Select(line => new RegisterBalance
            {
                OrganizationId  = sale.OrganizationId,
                DocumentTypeId  = DocumentTypeIdConst.SALE,
                DocumentId      = sale.Id,
                WarehouseId     = sale.WarehouseId,
                ProductId       = line.ProductId,
                ProductTableId  = null,
                OperationTypeId = OperationTypeIdConst.OUT,
                Quantity        = line.Quantity,
                Amount          = line.CostPrice,
                DocDate         = sale.DocDate,
                CreatedDate     = DateTime.Now,
                SourceLineId    = line.Id
            });

        var entries = trackedEntries
            .Concat(nonTrackedEntries)
            .ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
