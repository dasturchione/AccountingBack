using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleByListFilterCriteriaBuilder : ICriteriaBuilder<Role, RoleListFilter>
{
    public Expression<Func<Role, bool>> Build(RoleListFilter options)
    {
        return _ => true;
    }
}
