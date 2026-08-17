using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.InventoryAdjustments;

public sealed class InventoryAdjustmentListDtoOrderByBuilder : IOrderByBuilder<InventoryAdjustmentDoc, InventoryAdjustmentListDto>
{
    public Func<IQueryable<InventoryAdjustmentListDto>, IOrderedQueryable<InventoryAdjustmentListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
