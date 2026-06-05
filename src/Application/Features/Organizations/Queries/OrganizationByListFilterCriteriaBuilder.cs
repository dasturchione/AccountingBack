using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Organizations;

public class OrganizationByListFilterCriteriaBuilder : ICriteriaBuilder<Organization, OrganizationListFilter>
{
    public Expression<Func<Organization, bool>> Build(OrganizationListFilter options)
    {
        return o => (options.RegionId == null || o.RegionId == options.RegionId) &&
                    (options.IsParent == null || o.IsParent == options.IsParent);
    }
}
