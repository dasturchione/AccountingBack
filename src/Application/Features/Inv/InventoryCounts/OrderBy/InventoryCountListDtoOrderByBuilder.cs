using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.InventoryCounts;

public sealed class InventoryCountListDtoOrderByBuilder : IOrderByBuilder<InventoryCountDoc, InventoryCountListDto>
{
    public Func<IQueryable<InventoryCountListDto>, IOrderedQueryable<InventoryCountListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
