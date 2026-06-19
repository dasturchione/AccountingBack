using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class SaleInventoryHandler : IInventoryDocumentHandler<SaleDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(SaleDoc sale, CancellationToken ct = default)
    {
        var entries = sale.SaleDocTables.Select(line => new RegisterBalance
        {
            OrganizationId  = sale.OrganizationId,
            DocumentTypeId  = DocumentTypeIdConst.SALE,
            DocumentId      = sale.Id,
            WarehouseId     = sale.WarehouseId,
            ProductId       = line.ProductTable.ProductId,
            OperationTypeId = OperationTypeIdConst.OUT,
            Quantity        = line.Quantity,
            Amount          = line.TotalAmount,
            DocDate         = sale.DocDate,
            CreatedDate     = DateTime.UtcNow
        }).ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
