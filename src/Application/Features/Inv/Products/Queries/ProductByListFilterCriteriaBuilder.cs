using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductByListFilterCriteriaBuilder : ICriteriaBuilder<Product, ProductListFilter>
{
    private readonly IUserContext _userContext;
    public ProductByListFilterCriteriaBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public Expression<Func<Product, bool>> Build(ProductListFilter options) =>
        x => (!_userContext.OrganizationId.HasValue || x.OrganizationId == _userContext.OrganizationId.Value) &&
             (!options.ProductGroupId.HasValue || x.ProductGroupId == options.ProductGroupId.Value) &&
             (!options.IsPieceTracked.HasValue || x.IsService == options.IsPieceTracked.Value) && 
             (!options.IsService.HasValue || x.IsService == options.IsService.Value);
}
