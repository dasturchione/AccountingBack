using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryAdjustments;

public class InventoryAdjustmentDtoProjection : IProjectionBuilder<InventoryAdjustmentDoc, InventoryAdjustmentDto>
{
    public Expression<Func<InventoryAdjustmentDoc, InventoryAdjustmentDto>> Build() =>
        x => new InventoryAdjustmentDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            DocNumber = x.DocNumber,
            DocDate = x.DocDate,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            AdjustmentType = x.AdjustmentType,
            DirectionId = x.DirectionId,
            DirectionName = x.Direction.Name,
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
            Lines = x.InventoryAdjustmentLines.Select(l => new InventoryAdjustmentLineDto
            {
                Id = l.Id,
                OwnerId = l.OwnerId,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                Quantity = l.Quantity,
                Comment = l.Comment,
                Items = l.InventoryAdjustmentDocTables.Select(t => new InventoryAdjustmentTableDto
                {
                    Id = t.Id,
                    OwnerId = t.OwnerId,
                    ProductTableId = t.ProductTableId,
                    CostPrice = t.CostPrice,
                    MarkingNumber = t.ProductTable != null ? t.ProductTable.MarkingNumber : null,
                    SerialNumber = t.ProductTable != null ? t.ProductTable.SerialNumber : null,
                    WasCreated = t.WasCreated
                }).ToList()
            }).ToList()
        };
}
