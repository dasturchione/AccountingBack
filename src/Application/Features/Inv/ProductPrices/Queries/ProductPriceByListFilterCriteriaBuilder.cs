using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductPrices;

public class ProductPriceByListFilterCriteriaBuilder : ICriteriaBuilder<ProductPrice, ProductPriceListFilter>
{
    public Expression<Func<ProductPrice, bool>> Build(ProductPriceListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.ProductId.HasValue || x.ProductId == options.ProductId.Value);
}
