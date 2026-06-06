using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ProductGroups;

public class ProductGroupByIdCriteriaBuilder : ICriteriaBuilder<ProductGroup, GetByIdOptions<int>>
{
    public Expression<Func<ProductGroup, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
