using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ProductListDto, ProductListFilter>
{
    public Expression<Func<ProductListDto, bool>> Build(ProductListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower()) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(options.Search.ToLower()));
}
