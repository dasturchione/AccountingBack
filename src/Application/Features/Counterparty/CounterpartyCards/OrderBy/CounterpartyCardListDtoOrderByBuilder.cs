using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CounterpartyCards;

public sealed class CounterpartyCardListDtoOrderByBuilder : IOrderByBuilder<CounterpartyCard, CounterpartyCardListDto>
{
    public Func<IQueryable<CounterpartyCardListDto>, IOrderedQueryable<CounterpartyCardListDto>> Build() =>
        query => query.OrderBy(x => x.ShortName).ThenBy(x => x.Id);
}
