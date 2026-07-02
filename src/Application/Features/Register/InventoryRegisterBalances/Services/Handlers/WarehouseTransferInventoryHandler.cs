using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class WarehouseTransferInventoryHandler : IInventoryDocumentHandler<WarehouseTransferDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(WarehouseTransferDoc document, CancellationToken ct = default)
    {
        var entries = document.WarehouseTransferLines
            .SelectMany(line => line.WarehouseTransferDocTables.SelectMany(table => new[]
            {
                new RegisterBalance
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
                    DocumentId = document.Id,
                    WarehouseId = document.SourceWarehouseId,
                    ProductId = line.ProductId,
                    ProductTableId = table.ProductTableId,
                    OperationTypeId = OperationTypeIdConst.OUT,
                    Quantity = 1,
                    Amount = table.CostPrice,
                    DocDate = document.DocDate,
                    CreatedDate = DateTime.Now,
                    SourceLineId = table.Id
                },
                new RegisterBalance
                {
                    OrganizationId = document.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
                    DocumentId = document.Id,
                    WarehouseId = document.DestinationWarehouseId,
                    ProductId = line.ProductId,
                    ProductTableId = table.ProductTableId,
                    OperationTypeId = OperationTypeIdConst.IN,
                    Quantity = 1,
                    Amount = table.CostPrice,
                    DocDate = document.DocDate,
                    CreatedDate = DateTime.Now,
                    SourceLineId = table.Id
                }
            }))
            .ToList();

        return Task.FromResult(Result.Success(entries));
    }
}
