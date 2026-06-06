using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Products;

public class ProductByIdCriteriaBuilder : ICriteriaBuilder<Product, GetByIdOptions<int>>
{
    public Expression<Func<Product, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
