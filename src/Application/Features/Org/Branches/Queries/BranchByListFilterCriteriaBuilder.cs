using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Branches;

public class BranchByListFilterCriteriaBuilder : ICriteriaBuilder<Branch, BranchListFilter>
{
    public Expression<Func<Branch, bool>> Build(BranchListFilter options)
        => x => (options.OrganizationId == null || x.OrganizationId == options.OrganizationId) &&
                (options.RegionId == null || x.RegionId == options.RegionId);
}
