using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Branches;

public class BranchByListFilterCriteriaBuilder : ICriteriaBuilder<Branch, BranchListFilter>
{
    public Expression<Func<Branch, bool>> Build(BranchListFilter options)
    {
        return b => (options.OrganizationId == null || b.OrganizationId == options.OrganizationId) &&
                    (options.RegionId == null || b.RegionId == options.RegionId);
    }
}
