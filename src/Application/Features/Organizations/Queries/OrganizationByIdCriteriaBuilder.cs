using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Organizations;

public class OrganizationByIdCriteriaBuilder : ICriteriaBuilder<Organization, GetByIdOptions<int>>
{
    public Expression<Func<Organization, bool>> Build(GetByIdOptions<int> options)
    {
        return o => o.Id == options.Id;
    }
}
