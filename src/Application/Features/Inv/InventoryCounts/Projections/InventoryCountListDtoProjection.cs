using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryCounts;

public class InventoryCountListDtoProjection : IProjectionBuilder<InventoryCountDoc, InventoryCountListDto>
{
    public Expression<Func<InventoryCountDoc, InventoryCountListDto>> Build() =>
        x => new InventoryCountListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            CountCompletedAt = x.CountCompletedAt,
            PostedAt = x.PostedAt,
            CancelledAt = x.CancelledAt
        };
}
