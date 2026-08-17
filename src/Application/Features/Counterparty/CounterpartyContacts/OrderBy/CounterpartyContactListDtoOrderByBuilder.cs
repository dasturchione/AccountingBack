using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CounterpartyContacts;

public sealed class CounterpartyContactListDtoOrderByBuilder : IOrderByBuilder<CounterpartyContact, CounterpartyContactListDto>
{
    public Func<IQueryable<CounterpartyContactListDto>, IOrderedQueryable<CounterpartyContactListDto>> Build() =>
        query => query.OrderBy(x => x.FullName).ThenBy(x => x.Id);
}
