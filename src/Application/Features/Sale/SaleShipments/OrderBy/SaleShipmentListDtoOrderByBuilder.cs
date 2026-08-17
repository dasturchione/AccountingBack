using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.SaleShipments;

public sealed class SaleShipmentListDtoOrderByBuilder : IOrderByBuilder<SaleShipmentDoc, SaleShipmentListDto>
{
    public Func<IQueryable<SaleShipmentListDto>, IOrderedQueryable<SaleShipmentListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
