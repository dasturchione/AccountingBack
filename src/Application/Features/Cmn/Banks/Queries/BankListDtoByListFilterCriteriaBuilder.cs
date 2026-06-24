using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Banks;

public class BankListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<BankListDto, BankListFilter>
{
    public Expression<Func<BankListDto, bool>> Build(BankListFilter options) =>
        x => string.IsNullOrEmpty(options.Search) ||
             x.Code.ToLower().Contains(options.Search.ToLower()) ||
             x.Name.ToLower().Contains(options.Search.ToLower()) ||
             (x.Mfo != null && x.Mfo.ToLower().Contains(options.Search.ToLower()));
}
