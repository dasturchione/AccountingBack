using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleByIdCriteriaBuilder : ICriteriaBuilder<Role, GetByIdOptions<int>>
{
    public Expression<Func<Role, bool>> Build(GetByIdOptions<int> options)
    {
        return r => r.Id == options.Id;
    }
}
