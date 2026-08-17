using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Organizations;

public sealed class OrganizationListDtoOrderByBuilder : IOrderByBuilder<Organization, OrganizationListDto>
{
    public Func<IQueryable<OrganizationListDto>, IOrderedQueryable<OrganizationListDto>> Build() =>
        query => query.OrderBy(x => x.ShortName).ThenBy(x => x.Id);
}
