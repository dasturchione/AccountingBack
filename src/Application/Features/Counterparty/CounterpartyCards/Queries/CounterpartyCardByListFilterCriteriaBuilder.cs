using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public class CounterpartyCardByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyCard, CounterpartyCardListFilter>
{
    public Expression<Func<CounterpartyCard, bool>> Build(CounterpartyCardListFilter options) =>
        x => (!options.CounterpartyTypeId.HasValue || x.CounterpartyTypeId == options.CounterpartyTypeId.Value);
}
