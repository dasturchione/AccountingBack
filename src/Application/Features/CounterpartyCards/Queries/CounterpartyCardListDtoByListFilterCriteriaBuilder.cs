using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyCardListDto, CounterpartyCardListFilter>
{
    public Expression<Func<CounterpartyCardListDto, bool>> Build(CounterpartyCardListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.ShortName.ToLower().Contains(options.Search.ToLower()) ||
                (x.FullName != null && x.FullName.ToLower().Contains(options.Search.ToLower())) ||
                (x.Inn != null && x.Inn.ToLower().Contains(options.Search.ToLower()));
}
