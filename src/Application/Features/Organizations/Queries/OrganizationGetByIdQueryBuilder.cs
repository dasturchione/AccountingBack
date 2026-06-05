using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Organizations;

public class OrganizationGetByIdQueryBuilder : IQuerySpecificationBuilder<Organization, GetByIdOptions<int>>
{
    public QuerySpecification<Organization> Build(GetByIdOptions<int> filter)
    {
        return new QuerySpecification<Organization>
        {
            Criteria = o => o.Id == filter.Id
        };
    }
}
