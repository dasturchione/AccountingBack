using Application.Abstractions.Authentication;
using Application.Specifications;
using Domain.Entities;

namespace Application.Features.Users
{
    public class UserListQueryBuilder : IPagedQuerySpecificationBuilder<User, UserListDto, UserListFilter>
    {
        private readonly UserListDtoMap _map = new UserListDtoMap();
        private readonly IUserContext _userContext;
        public UserListQueryBuilder(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public PagedQuerySpecification<User, UserListDto> Build(UserListFilter filter)
        {
            return new PagedQuerySpecification<User, UserListDto>()
            {
                Criteria = u => (filter.RoleId == null || u.RoleId == filter.RoleId),
                OrderBy = q => q.OrderBy(u => u.UserName),
                ResultCriteria = x => (string.IsNullOrEmpty(filter.Search) ||
                                         x.UserName.ToLower().Contains(filter.Search.ToLower()) ||
                                         x.FirstName.ToLower().Contains(filter.Search.ToLower()) ||
                                         x.LastName.ToLower().Contains(filter.Search.ToLower())),
                Selector = _map.Build(),
                Skip = (filter.Page - 1) * (filter.PageSize ?? 20),
                Take = filter.PageSize ?? 20
            };
        }
    }
}
