using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Organizations;

public class OrganizationListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<OrganizationListDto, OrganizationListFilter>
{
    public Expression<Func<OrganizationListDto, bool>> Build(OrganizationListFilter options)
    {
        return x => string.IsNullOrEmpty(options.Search) ||
                    x.ShortName.ToLower().Contains(options.Search.ToLower()) ||
                    x.FullName.ToLower().Contains(options.Search.ToLower()) ||
                    x.Inn.ToLower().Contains(options.Search.ToLower());
    }
}
