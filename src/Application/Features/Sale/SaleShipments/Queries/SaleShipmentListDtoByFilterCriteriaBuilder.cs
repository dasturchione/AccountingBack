using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentListDtoByFilterCriteriaBuilder : ICriteriaBuilder<SaleShipmentListDto, SaleShipmentFilter>
{
    public Expression<Func<SaleShipmentListDto, bool>> Build(SaleShipmentFilter filter) =>
        shipment => (!filter.WarehouseId.HasValue || shipment.WarehouseId == filter.WarehouseId.Value) &&
                    (!filter.CounterpartyId.HasValue || shipment.CounterpartyId == filter.CounterpartyId.Value) &&
                    (!filter.StatusId.HasValue || shipment.StatusId == filter.StatusId.Value) &&
                    (!filter.DateFrom.HasValue || shipment.DocDate >= filter.DateFrom.Value) &&
                    (!filter.DateTo.HasValue || shipment.DocDate <= filter.DateTo.Value) &&
                    (string.IsNullOrWhiteSpace(filter.Search) || (shipment.DocNumber != null && shipment.DocNumber.Contains(filter.Search)));
}
