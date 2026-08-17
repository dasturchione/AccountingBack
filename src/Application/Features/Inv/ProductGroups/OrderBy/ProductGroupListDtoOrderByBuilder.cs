using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.ProductGroups;

public sealed class ProductGroupListDtoOrderByBuilder : IOrderByBuilder<ProductGroup, ProductGroupListDto>
{
    public Func<IQueryable<ProductGroupListDto>, IOrderedQueryable<ProductGroupListDto>> Build() =>
        query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ThenBy(x => x.Id);
}
