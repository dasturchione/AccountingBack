using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.Cmn.Currencies;

public sealed class CurrencyListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CurrencyListDto, CurrencyListFilter>
{
    public Expression<Func<CurrencyListDto, bool>> Build(CurrencyListFilter options) =>
        x => string.IsNullOrWhiteSpace(options.Search) ||
             x.Code.ToLower().Contains(options.Search.ToLower()) ||
             x.Name.ToLower().Contains(options.Search.ToLower()) ||
             (x.Symbol != null && x.Symbol.ToLower().Contains(options.Search.ToLower()));
}
