using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Branches;

public class BranchByIdCriteriaBuilder : ICriteriaBuilder<Branch, GetByIdOptions<int>>
{
    public Expression<Func<Branch, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
