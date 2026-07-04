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
             (!options.ProductTypeId.HasValue || x.ProductTypeId == options.ProductTypeId.Value) &&
             (!options.IsPieceTracked.HasValue || x.IsPieceTracked == options.IsPieceTracked.Value) &&
             (!options.IsService.HasValue || x.IsService == options.IsService.Value) &&
             (!options.IsSold.HasValue || x.IsSold == options.IsSold.Value) &&
             (!options.IsPurchased.HasValue || x.IsPurchased == options.IsPurchased.Value);
}
