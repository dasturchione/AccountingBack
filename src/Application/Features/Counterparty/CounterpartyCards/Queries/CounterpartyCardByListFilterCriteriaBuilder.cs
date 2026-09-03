using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyCards;

public sealed class CounterpartyCardByListFilterCriteriaBuilder
    : ICriteriaBuilder<CounterpartyCard, CounterpartyCardListFilter>
{
    public Expression<Func<CounterpartyCard, bool>> Build(CounterpartyCardListFilter options) =>
        counterparty => !options.OrganizationId.HasValue ||
                        counterparty.OrganizationId == options.OrganizationId.Value;
}
