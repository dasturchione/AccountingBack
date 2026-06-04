using Application.Abstractions.Authentication;
using Application.Options;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Users;

public class UserDtoQueryBuilder : IQuerySpecificationBuilder<User, UserDto, GetByIdOptions<int>>
{
    private readonly UserDtoMap _map = new UserDtoMap();
    private readonly IUserContext _userContext;
    public UserDtoQueryBuilder(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public QuerySpecification<User, UserDto> Build(GetByIdOptions<int> option)
    {
        return new QuerySpecification<User, UserDto>()
        {
            Criteria = x => x.Id == option.Id,
            Selector = _map.Build()
        };
    }
}
