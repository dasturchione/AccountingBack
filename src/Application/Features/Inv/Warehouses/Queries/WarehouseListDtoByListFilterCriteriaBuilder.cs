using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Warehouses;

public class WarehouseListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<WarehouseListDto, WarehouseListFilter>
{
    public Expression<Func<WarehouseListDto, bool>> Build(WarehouseListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
