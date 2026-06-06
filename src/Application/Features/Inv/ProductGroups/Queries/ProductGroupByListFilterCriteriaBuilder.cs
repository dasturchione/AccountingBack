using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupByListFilterCriteriaBuilder : ICriteriaBuilder<ProductGroup, ProductGroupListFilter>
{
    public Expression<Func<ProductGroup, bool>> Build(ProductGroupListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.ParentId.HasValue || x.ParentId == options.ParentId.Value);
}
