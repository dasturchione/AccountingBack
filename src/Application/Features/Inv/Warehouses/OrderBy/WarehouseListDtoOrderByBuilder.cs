using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Warehouses;

public sealed class WarehouseListDtoOrderByBuilder : IOrderByBuilder<Warehouse, WarehouseListDto>
{
    public Func<IQueryable<WarehouseListDto>, IOrderedQueryable<WarehouseListDto>> Build() =>
        query => query.OrderByDescending(x => x.IsMain).ThenBy(x => x.Name).ThenBy(x => x.Id);
}
