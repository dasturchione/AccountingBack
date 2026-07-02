using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentListDtoProjection : IProjectionBuilder<InventoryAdjustmentDoc, InventoryAdjustmentListDto>
{
    public Expression<Func<InventoryAdjustmentDoc, InventoryAdjustmentListDto>> Build() =>
        x => new InventoryAdjustmentListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            AdjustmentType = x.AdjustmentType,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            Comment = x.Comment,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId
        };
}
