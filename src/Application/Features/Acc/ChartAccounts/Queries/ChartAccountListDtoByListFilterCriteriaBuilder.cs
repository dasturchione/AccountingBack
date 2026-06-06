using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ChartAccountListDto, ChartAccountListFilter>
{
    public Expression<Func<ChartAccountListDto, bool>> Build(ChartAccountListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
