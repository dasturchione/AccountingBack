using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Users;

public sealed class UserListDtoOrderByBuilder : IOrderByBuilder<User, UserListDto>
{
    public Func<IQueryable<UserListDto>, IOrderedQueryable<UserListDto>> Build() =>
        query => query.OrderBy(x => x.UserName).ThenBy(x => x.Id);
}
