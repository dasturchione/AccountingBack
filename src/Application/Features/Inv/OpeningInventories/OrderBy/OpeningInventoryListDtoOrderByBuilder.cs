using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Inv.OpeningInventories;

public sealed class OpeningInventoryListDtoOrderByBuilder : IOrderByBuilder<OpeningInventory, OpeningInventoryListDto>
{
    public Func<IQueryable<OpeningInventoryListDto>, IOrderedQueryable<OpeningInventoryListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
