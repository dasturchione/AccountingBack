using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Roles;

public class RoleListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<RoleListDto, RoleListFilter>
{
    public Expression<Func<RoleListDto, bool>> Build(RoleListFilter options)
    {
        return x => string.IsNullOrEmpty(options.Search) ||
                    x.ShortName.ToLower().Contains(options.Search.ToLower()) ||
                    x.FullName.ToLower().Contains(options.Search.ToLower());
    }
}
