using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Products;

public sealed class ProductListDtoOrderByBuilder : IOrderByBuilder<Product, ProductListDto>
{
    public Func<IQueryable<ProductListDto>, IOrderedQueryable<ProductListDto>> Build() =>
        query => query.OrderBy(x => x.Name).ThenBy(x => x.Id);
}
