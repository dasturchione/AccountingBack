using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryMovements;

public sealed class InventoryMovementListProjection : IProjectionBuilder<WarehouseProductMovement, InventoryMovementListDto>
{
    public Expression<Func<WarehouseProductMovement, InventoryMovementListDto>> Build() =>
        x => new InventoryMovementListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            WarehouseId = x.WarehouseId,
            ProductId = x.ProductId,
            OperationTypeId = x.MovementSign == 1
                ? OperationTypeIdConst.IN
                : OperationTypeIdConst.OUT,
            Quantity = x.Quantity,
            Amount = x.MovementSign == 1
                ? x.WarehouseProductBatch == null
                    ? 0m
                    : x.WarehouseProductBatch.InitialQuantity * (x.WarehouseProductBatch.UnitCost ?? 0m)
                : x.WarehouseProductBatchAllocations.Sum(allocation =>
                    allocation.Quantity * (allocation.UnitCost ?? 0m)),
            DocDate = x.MovementDate,
            SourceLineId = x.DocumentLineId,
            CreatedDate = x.CreatedDate
        };
}
