using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.InventoryMovements;

public sealed class InventoryMovementListDtoOrderByBuilder : IOrderByBuilder<WarehouseProductMovement, InventoryMovementListDto>
{
    public Func<IQueryable<InventoryMovementListDto>, IOrderedQueryable<InventoryMovementListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
