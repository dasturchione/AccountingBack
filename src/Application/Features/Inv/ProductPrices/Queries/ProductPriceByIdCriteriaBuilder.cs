using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductPrices;

public class ProductPriceByIdCriteriaBuilder : ICriteriaBuilder<ProductPrice, GetByIdOptions<long>>
{
    public Expression<Func<ProductPrice, bool>> Build(GetByIdOptions<long> options) => x => x.Id == options.Id;
}
