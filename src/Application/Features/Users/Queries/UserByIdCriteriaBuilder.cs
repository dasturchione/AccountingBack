using Domain.Entities;
﻿using Application.Options;

using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Users
{
    public class UserByIdCriteriaBuilder : ICriteriaBuilder<User, GetByIdOptions<int>>
    {
        public Expression<Func<User, bool>> Build(GetByIdOptions<int> options)
        {
            return x => x.Id == options.Id;
        } 
    }
}
