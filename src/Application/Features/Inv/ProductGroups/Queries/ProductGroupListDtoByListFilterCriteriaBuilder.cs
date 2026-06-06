using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ProductGroupListDto, ProductGroupListFilter>
{
    public Expression<Func<ProductGroupListDto, bool>> Build(ProductGroupListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
