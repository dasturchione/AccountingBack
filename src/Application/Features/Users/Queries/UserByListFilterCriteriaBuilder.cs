using Domain.Entities;
﻿
using LinqKit;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users.Queries
{
    public class UserByListFilterCriteriaBuilder : ICriteriaBuilder<User, UserListFilter>
    {
        public Expression<Func<User, bool>> Build(UserListFilter options)
        {
            Expression<Func<User, bool>> predicate = x => true;

            if (options.RoleId.HasValue)
            {
                predicate = predicate.And(x => x.RoleId == options.RoleId.Value);
            }
            return predicate;
        }
    }
}
