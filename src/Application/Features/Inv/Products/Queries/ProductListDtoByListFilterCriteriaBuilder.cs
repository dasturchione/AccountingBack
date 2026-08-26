using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ProductListDto, ProductListFilter>
{
    public Expression<Func<ProductListDto, bool>> Build(ProductListFilter options)
    {
        var search = options.Search?.Trim().ToLower();

        return x => string.IsNullOrEmpty(search) ||
                    x.Name.ToLower().Contains(search) ||
                    (x.Barcode != null && x.Barcode.ToLower().Contains(search)) ||
                    (x.Code != null && x.Code.ToLower().Contains(search)) ||
                    (x.Sku != null && x.Sku.ToLower().Contains(search)) ||
                    (x.Article != null && x.Article.ToLower().Contains(search)) ||
                    (x.Mxik != null && x.Mxik.ToLower().Contains(search));
    }
}
