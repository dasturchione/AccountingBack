using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryCounts;

public class InventoryCountDtoProjection : IProjectionBuilder<InventoryCountDoc, InventoryCountDto>
{
    public Expression<Func<InventoryCountDoc, InventoryCountDto>> Build() =>
        x => new InventoryCountDto
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
            Comment = x.Comment,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate,
            CountCompletedAt = x.CountCompletedAt,
            CountCompletedByUserId = x.CountCompletedByUserId,
            PositiveAdjustmentDocId = x.PositiveAdjustmentDocId,
            NegativeAdjustmentDocId = x.NegativeAdjustmentDocId,
            PostedAt = x.PostedAt,
            PostedByUserId = x.PostedByUserId,
            CancelledAt = x.CancelledAt,
            CancelledByUserId = x.CancelledByUserId,
            Lines = x.InventoryCountLines.Select(l => new InventoryCountLineDto
            {
                Id = l.Id,
                OwnerId = l.OwnerId,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                CountedQuantity = l.CountedQuantity,
                DefaultCostPrice = l.DefaultCostPrice,
                Comment = l.Comment,
                Items = l.InventoryCountDocTables.Select(t => new InventoryCountTableDto
                {
                    Id = t.Id,
                    OwnerId = t.OwnerId,
                    ProductTableId = t.ProductTableId,
                    Barcode = t.Barcode,
                    SerialNumber = t.SerialNumber,
                    MarkingNumber = t.MarkingNumber,
                    CostPrice = t.CostPrice
                }).ToList()
            }).ToList()
        };
}
