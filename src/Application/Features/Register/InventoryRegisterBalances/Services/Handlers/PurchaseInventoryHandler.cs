using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class PurchaseInventoryHandler : IInventoryDocumentHandler<PurchaseDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
    {
        var entries = purchase.PurchaseDocTables.Select(line => new RegisterBalance
        {
            OrganizationId  = purchase.OrganizationId,
            DocumentTypeId  = DocumentTypeIdConst.PURCHASE,
            DocumentId      = purchase.Id,
            WarehouseId     = purchase.WarehouseId,
            ProductId       = line.ProductTable.ProductId,
            OperationTypeId = OperationTypeIdConst.IN,
            Quantity        = line.Quantity,
            Amount          = line.TotalAmount,
            DocDate         = purchase.DocDate,
            CreatedDate     = DateTime.Now
        }).ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
