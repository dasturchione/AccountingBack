using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CashBoxes;

public class CashBoxListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CashBoxListDto, CashBoxListFilter>
{
    public Expression<Func<CashBoxListDto, bool>> Build(CashBoxListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.Name.ToLower().Contains(options.Search.ToLower()) ||
                x.Code.ToLower().Contains(options.Search.ToLower());
}
