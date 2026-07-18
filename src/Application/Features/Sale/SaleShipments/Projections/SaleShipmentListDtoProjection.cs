using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentListDtoProjection : IProjectionBuilder<SaleShipmentDoc, SaleShipmentListDto>
{
    public Expression<Func<SaleShipmentDoc, SaleShipmentListDto>> Build() =>
        shipment => new SaleShipmentListDto
        {
            Id = shipment.Id,
            SaleDocId = shipment.SaleDocId,
            WarehouseId = shipment.WarehouseId,
            WarehouseName = shipment.Warehouse.Name,
            CounterpartyId = shipment.CounterpartyId,
            CounterpartyName = shipment.Counterparty == null ? null : shipment.Counterparty.ShortName,
            DocNumber = shipment.DocNumber,
            DocDate = shipment.DocDate,
            StatusId = shipment.StatusId,
            StatusName = shipment.Status.Name,
            Comment = shipment.Comment,
            CreatedDate = shipment.CreatedDate
        };
}
