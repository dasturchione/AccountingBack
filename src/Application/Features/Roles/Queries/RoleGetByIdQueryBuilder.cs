using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Roles;

public class RoleGetByIdQueryBuilder : IQuerySpecificationBuilder<Role, GetByIdOptions<int>>
{
    public QuerySpecification<Role> Build(GetByIdOptions<int> filter)
    {
        return new QuerySpecification<Role>
        {
            Criteria = r => r.Id == filter.Id
        };
    }
}
