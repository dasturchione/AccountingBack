using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Roles;

public class RoleListQueryBuilder : IPagedQuerySpecificationBuilder<Role, RoleListDto, RoleListFilter>
{
    private readonly RoleListDtoMap _map = new();

    public PagedQuerySpecification<Role, RoleListDto> Build(RoleListFilter filter)
    {
        return new PagedQuerySpecification<Role, RoleListDto>
        {
            Criteria        = _ => true,
            OrderBy         = q => q.OrderBy(r => r.FullName),
            ResultCriteria  = x => string.IsNullOrEmpty(filter.Search) ||
                                   x.ShortName.ToLower().Contains(filter.Search.ToLower()) ||
                                   x.FullName.ToLower().Contains(filter.Search.ToLower()),
            Selector        = _map.Build(),
            Skip            = (filter.Page - 1) * (filter.PageSize ?? 20),
            Take            = filter.PageSize ?? 20
        };
    }
}
