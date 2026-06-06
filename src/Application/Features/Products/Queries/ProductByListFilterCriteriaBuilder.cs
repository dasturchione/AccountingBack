using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductByListFilterCriteriaBuilder : ICriteriaBuilder<Product, ProductListFilter>
{
    public Expression<Func<Product, bool>> Build(ProductListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.ProductGroupId.HasValue || x.ProductGroupId == options.ProductGroupId.Value) &&
             (!options.IsService.HasValue || x.IsService == options.IsService.Value);
}
