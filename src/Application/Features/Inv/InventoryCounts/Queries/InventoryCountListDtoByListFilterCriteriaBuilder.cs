using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.InventoryCounts;

public class InventoryCountListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<InventoryCountListDto, InventoryCountListFilter>
{
    public Expression<Func<InventoryCountListDto, bool>> Build(InventoryCountListFilter options) =>
        x =>
            (!options.WarehouseId.HasValue || x.WarehouseId == options.WarehouseId.Value) &&
            (!options.StatusId.HasValue || x.StatusId == options.StatusId.Value) &&
            (string.IsNullOrWhiteSpace(options.Search) ||
             x.DocNumber.ToLower().Contains(options.Search.ToLower()) ||
             x.WarehouseName.ToLower().Contains(options.Search.ToLower()));
}
