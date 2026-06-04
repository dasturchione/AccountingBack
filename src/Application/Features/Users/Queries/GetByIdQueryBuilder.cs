using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Users.Queries;

public class GetByIdQueryBuilder : IQuerySpecificationBuilder<User, GetByIdOptions<int>>
{
    public QuerySpecification<User> Build(GetByIdOptions<int> filter)
    {
        return new QuerySpecification<User>
        {
            Criteria = user => user.Id == filter.Id
        };
    }
}
