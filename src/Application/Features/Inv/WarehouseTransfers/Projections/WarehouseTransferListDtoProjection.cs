using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferListDtoProjection : IProjectionBuilder<WarehouseTransferDoc, WarehouseTransferListDto>
{
    public Expression<Func<WarehouseTransferDoc, WarehouseTransferListDto>> Build() =>
        x => new WarehouseTransferListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            SourceWarehouseId = x.SourceWarehouseId,
            SourceWarehouseName = x.SourceWarehouse.Name,
            DestinationWarehouseId = x.DestinationWarehouseId,
            DestinationWarehouseName = x.DestinationWarehouse.Name,
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
