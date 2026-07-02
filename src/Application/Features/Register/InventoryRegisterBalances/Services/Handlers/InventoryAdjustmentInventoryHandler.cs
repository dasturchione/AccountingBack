using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryAdjustmentInventoryHandler : IInventoryDocumentHandler<InventoryAdjustmentDoc>
{
    public Task<Result<List<RegisterBalance>>> HandleAsync(InventoryAdjustmentDoc document, CancellationToken ct = default)
    {
        var operationTypeId = IsPositiveFlow(document.AdjustmentType)
            ? OperationTypeIdConst.IN
            : OperationTypeIdConst.OUT;

        var entries = document.InventoryAdjustmentLines
            .SelectMany(line => line.InventoryAdjustmentDocTables.Select(table => new RegisterBalance
            {
                OrganizationId = document.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.INVENTORYADJUSTMENT,
                DocumentId = document.Id,
                WarehouseId = document.WarehouseId,
                ProductId = line.ProductId,
                ProductTableId = table.ProductTableId,
                OperationTypeId = operationTypeId,
                Quantity = 1,
                Amount = table.CostPrice,
                DocDate = document.DocDate,
                CreatedDate = DateTime.Now,
                SourceLineId = table.Id
            }))
            .ToList();

        return Task.FromResult(Result.Success(entries));
    }

    private static bool IsPositiveFlow(string adjustmentType) =>
        adjustmentType is "POSITIVE_ADJUSTMENT" or "FOUND_STOCK" or "CORRECTION";
}
