using Microsoft.EntityFrameworkCore;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.ChartAccounts;

public class ChartAccountListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<ChartAccountListDto, ChartAccountListFilter>
{
    public Expression<Func<ChartAccountListDto, bool>> Build(ChartAccountListFilter options)
    {
        var search = options.Search?.Trim();
        var pattern = string.IsNullOrWhiteSpace(search) ? null : $"{search}%";

        return x => pattern == null ||
                    EF.Functions.Like(x.Name, pattern) ||
                    EF.Functions.Like(x.Code, pattern);
    }
}
