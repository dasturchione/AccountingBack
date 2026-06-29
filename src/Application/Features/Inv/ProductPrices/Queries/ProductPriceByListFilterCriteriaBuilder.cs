using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Inv.ProductPrices;

public class ProductPriceByListFilterCriteriaBuilder : ICriteriaBuilder<ProductPrice, ProductPriceListFilter>
{
    public Expression<Func<ProductPrice, bool>> Build(ProductPriceListFilter options) =>
        x => (!options.ProductId.HasValue || x.ProductId == options.ProductId.Value)
          && (!options.PriceTypeId.HasValue || x.PriceTypeId == options.PriceTypeId.Value)
          && (!options.UnitId.HasValue || x.UnitId == options.UnitId.Value);
}
