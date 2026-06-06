using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductPrices;

public class ProductPriceListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ProductPriceListDto, ProductPriceListFilter>
{
    public Expression<Func<ProductPriceListDto, bool>> Build(ProductPriceListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.ProductName.ToLower().Contains(options.Search.ToLower());
}
