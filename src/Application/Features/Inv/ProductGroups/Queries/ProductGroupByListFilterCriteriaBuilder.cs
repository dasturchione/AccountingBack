using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupByListFilterCriteriaBuilder : ICriteriaBuilder<ProductGroup, ProductGroupListFilter>
{
    public Expression<Func<ProductGroup, bool>> Build(ProductGroupListFilter options) =>
        group => group.IsAssignable &&
                 (options.IsService == null || group.Products.Any(product => product.IsService == options.IsService));
}
