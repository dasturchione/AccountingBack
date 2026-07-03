using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.WarehouseTransfers;

public class WarehouseTransferDtoProjection : IProjectionBuilder<WarehouseTransferDoc, WarehouseTransferDto>
{
    public Expression<Func<WarehouseTransferDoc, WarehouseTransferDto>> Build() =>
        x => new WarehouseTransferDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            SourceWarehouseId = x.SourceWarehouseId,
            SourceWarehouseName = x.SourceWarehouse.Name,
            DestinationWarehouseId = x.DestinationWarehouseId,
            DestinationWarehouseName = x.DestinationWarehouse.Name,
            StatusId = x.StatusId,
            StatusName = x.Status.Name,
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Lines = x.WarehouseTransferLines.Select(l => new WarehouseTransferLineDto
            {
                Id = l.Id,
                OwnerId = l.OwnerId,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                Quantity = l.Quantity,
                Comment = l.Comment,
                Items = l.WarehouseTransferDocTables.Select(t => new WarehouseTransferTableDto
                {
                    Id = t.Id,
                    OwnerId = t.OwnerId,
                    ProductTableId = t.ProductTableId,
                    SourceWarehouseId = t.SourceWarehouseId,
                    DestinationWarehouseId = t.DestinationWarehouseId,
                    CostPrice = t.CostPrice,
                    MarkingNumber = t.ProductTable.MarkingNumber,
                    SerialNumber = t.ProductTable.SerialNumber
                }).ToList()
            }).ToList()
        };
}
