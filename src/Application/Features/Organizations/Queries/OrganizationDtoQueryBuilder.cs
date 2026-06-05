using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Organizations;

public class OrganizationDtoQueryBuilder : IQuerySpecificationBuilder<Organization, OrganizationDto, GetByIdOptions<int>>
{
    private readonly OrganizationDtoMap _map = new();

    public QuerySpecification<Organization, OrganizationDto> Build(GetByIdOptions<int> option)
    {
        return new QuerySpecification<Organization, OrganizationDto>
        {
            Criteria = o => o.Id == option.Id,
            Selector = _map.Build()
        };
    }
}
