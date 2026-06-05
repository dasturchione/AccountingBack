using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Roles;

public class RoleDtoQueryBuilder : IQuerySpecificationBuilder<Role, RoleDto, GetByIdOptions<int>>
{
    private readonly RoleDtoMap _map = new();

    public QuerySpecification<Role, RoleDto> Build(GetByIdOptions<int> option)
    {
        return new QuerySpecification<Role, RoleDto>
        {
            Criteria = r => r.Id == option.Id,
            Selector = _map.Build()
        };
    }
}
