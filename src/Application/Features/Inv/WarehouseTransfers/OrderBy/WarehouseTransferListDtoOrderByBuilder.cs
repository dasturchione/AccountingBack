using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.WarehouseTransfers;

public sealed class WarehouseTransferListDtoOrderByBuilder : IOrderByBuilder<WarehouseTransferDoc, WarehouseTransferListDto>
{
    public Func<IQueryable<WarehouseTransferListDto>, IOrderedQueryable<WarehouseTransferListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
