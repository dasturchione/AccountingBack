using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.Roles;

public sealed class RoleListDtoOrderByBuilder : IOrderByBuilder<Role, RoleListDto>
{
    public Func<IQueryable<RoleListDto>, IOrderedQueryable<RoleListDto>> Build() =>
        query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Id);
}
