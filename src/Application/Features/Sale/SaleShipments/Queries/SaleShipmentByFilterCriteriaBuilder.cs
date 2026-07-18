using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentByFilterCriteriaBuilder : ICriteriaBuilder<SaleShipmentDoc, SaleShipmentFilter>
{
    private readonly IUserContext _userContext;

    public SaleShipmentByFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<SaleShipmentDoc, bool>> Build(SaleShipmentFilter filter) =>
        shipment => (!_userContext.OrganizationId.HasValue || shipment.OrganizationId == _userContext.OrganizationId.Value) &&
                    (!filter.WarehouseId.HasValue || shipment.WarehouseId == filter.WarehouseId.Value) &&
                    (!filter.CounterpartyId.HasValue || shipment.CounterpartyId == filter.CounterpartyId.Value) &&
                    (!filter.StatusId.HasValue || shipment.StatusId == filter.StatusId.Value) &&
                    (!filter.DateFrom.HasValue || shipment.DocDate >= filter.DateFrom.Value) &&
                    (!filter.DateTo.HasValue || shipment.DocDate <= filter.DateTo.Value) &&
                    (string.IsNullOrWhiteSpace(filter.Search) || (shipment.DocNumber != null && shipment.DocNumber.Contains(filter.Search)));
}
