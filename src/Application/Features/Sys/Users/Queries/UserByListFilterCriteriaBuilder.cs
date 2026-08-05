using Domain.Entities;
using LinqKit;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users.Queries;

public class UserByListFilterCriteriaBuilder : ICriteriaBuilder<User, UserListFilter>
{
    public Expression<Func<User, bool>> Build(UserListFilter options)
    {
        Expression<Func<User, bool>> predicate = user => true;

        if (options.RoleId.HasValue)
            predicate = predicate.And(user => user.UserOrganizations.Any(organization => organization.RoleId == options.RoleId.Value));

        if (options.UserKindId.HasValue)
            predicate = predicate.And(user => user.UserKindId == options.UserKindId.Value);

        return predicate;
    }
}