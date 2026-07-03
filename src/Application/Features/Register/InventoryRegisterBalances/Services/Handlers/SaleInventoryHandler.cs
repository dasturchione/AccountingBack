using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class SaleInventoryHandler : IInventoryDocumentHandler<SaleDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
    {
        var allTables = sale.SaleDocProducts
            .Where(p => !p.Product.IsService)
            .SelectMany(p => p.SaleDocTables)
            .ToList();

        var entries = allTables.Select(line => new RegisterBalance
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
        }).ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
