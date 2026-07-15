using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class WarehouseTransferInventoryHandler : IInventoryDocumentHandler<WarehouseTransferDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(WarehouseTransferDoc document, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var entries = new List<RegisterBalance>();

        foreach (var line in document.WarehouseTransferLines.Where(line => !line.Product.IsService))
        {
            if (!line.Product.IsPieceTracked)
            {
                entries.Add(CreateEntry(document, line, document.SourceWarehouseId, OperationTypeIdConst.OUT, null, line.Quantity, 0m, line.Id, now));
                entries.Add(CreateEntry(document, line, document.DestinationWarehouseId, OperationTypeIdConst.IN, null, line.Quantity, 0m, line.Id, now));
                continue;
            }

            foreach (var table in line.WarehouseTransferDocTables)
            {
                entries.Add(CreateEntry(document, line, document.SourceWarehouseId, OperationTypeIdConst.OUT, table.ProductTableId, 1m, table.CostPrice, table.Id, now));
                entries.Add(CreateEntry(document, line, document.DestinationWarehouseId, OperationTypeIdConst.IN, table.ProductTableId, 1m, table.CostPrice, table.Id, now));
            }
        }

        return Task.FromResult(Result.Success(entries));
    }

    private static RegisterBalance CreateEntry(
        WarehouseTransferDoc document,
        WarehouseTransferLine line,
        int warehouseId,
        short operationTypeId,
        int? productTableId,
        decimal quantity,
        decimal amount,
        long sourceLineId,
        DateTime now) =>
        new()
        {
            OrganizationId = document.OrganizationId,
            DocumentTypeId = DocumentTypeIdConst.WAREHOUSETRANSFER,
            DocumentId = document.Id,
            WarehouseId = warehouseId,
            ProductId = line.ProductId,
            ProductTableId = productTableId,
            OperationTypeId = operationTypeId,
            Quantity = quantity,
            Amount = amount,
            DocDate = document.DocDate,
            CreatedDate = now,
            SourceLineId = sourceLineId
        };
}
