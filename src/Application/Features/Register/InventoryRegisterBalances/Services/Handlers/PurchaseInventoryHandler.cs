using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class PurchaseInventoryHandler : IInventoryDocumentHandler<PurchaseDoc>
{
    public Task<Result<List<InventoryRegisterBalance>>> HandleAsync(PurchaseDoc purchase, CancellationToken ct = default)
    {
        var entries = purchase.Lines.Select(line => new InventoryRegisterBalance
        {
            OrganizationId  = purchase.OrganizationId,
            DocumentTypeId  = DocumentTypeIdConst.PURCHASE,
            DocumentId      = purchase.Id,
            WarehouseId     = purchase.WarehouseId,
            ProductId       = line.ProductId,
            OperationTypeId = OperationTypeIdConst.IN,
            Quantity        = line.Quantity,
            Amount          = line.TotalAmount,
            DocDate         = purchase.DocDate,
            CreatedDate     = DateTime.UtcNow
        }).ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
