using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Inv.ProductPrices;

public sealed class ProductPriceListDtoOrderByBuilder : IOrderByBuilder<ProductPrice, ProductPriceListDto>
{
    public Func<IQueryable<ProductPriceListDto>, IOrderedQueryable<ProductPriceListDto>> Build() =>
        query => query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id);
}
