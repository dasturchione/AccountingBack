using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupByListFilterCriteriaBuilder : ICriteriaBuilder<ProductGroup, ProductGroupListFilter>
{
    private readonly IUserContext _userContext;
    public ProductGroupByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<ProductGroup, bool>> Build(ProductGroupListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) && 
             (options.IsService == null || x.Products.Any(a => a.IsService));
}
