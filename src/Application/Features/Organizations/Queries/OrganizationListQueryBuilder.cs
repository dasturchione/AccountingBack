using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Organizations;

public class OrganizationListQueryBuilder : IPagedQuerySpecificationBuilder<Organization, OrganizationListDto, OrganizationListFilter>
{
    private readonly OrganizationListDtoMap _map = new();

    public PagedQuerySpecification<Organization, OrganizationListDto> Build(OrganizationListFilter filter)
    {
        return new PagedQuerySpecification<Organization, OrganizationListDto>
        {
            Criteria       = o => (filter.RegionId == null || o.RegionId == filter.RegionId) &&
                                  (filter.IsParent == null || o.IsParent == filter.IsParent),
            OrderBy        = q => q.OrderBy(o => o.ShortName),
            ResultCriteria = x => string.IsNullOrEmpty(filter.Search) ||
                                  x.ShortName.ToLower().Contains(filter.Search.ToLower()) ||
                                  x.FullName.ToLower().Contains(filter.Search.ToLower()) ||
                                  x.Inn.ToLower().Contains(filter.Search.ToLower()),
            Selector       = _map.Build(),
            Skip           = (filter.Page - 1) * (filter.PageSize ?? 20),
            Take           = filter.PageSize ?? 20
        };
    }
}
