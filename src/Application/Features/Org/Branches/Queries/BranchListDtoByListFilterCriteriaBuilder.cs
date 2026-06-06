using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Branches;

public class BranchListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<BranchListDto, BranchListFilter>
{
    public Expression<Func<BranchListDto, bool>> Build(BranchListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
