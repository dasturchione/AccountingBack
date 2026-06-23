using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class SaleInventoryHandler : IInventoryDocumentHandler<SaleDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
    {
        var allTables = sale.SaleDocProducts.SelectMany(p => p.SaleDocTables).ToList();

        var entries = allTables.Select(line => new RegisterBalance
        {
            OrganizationId  = sale.OrganizationId,
            DocumentTypeId  = DocumentTypeIdConst.SALE,
            DocumentId      = sale.Id,
            WarehouseId     = sale.WarehouseId,
            ProductId       = line.ProductTable.ProductId,
            OperationTypeId = OperationTypeIdConst.OUT,
            Quantity        = 1,
            Amount          = line.TotalAmount,
            DocDate         = sale.DocDate,
            CreatedDate     = DateTime.Now
        }).ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
