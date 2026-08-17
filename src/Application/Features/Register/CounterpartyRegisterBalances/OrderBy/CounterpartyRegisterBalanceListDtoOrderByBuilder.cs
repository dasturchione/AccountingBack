using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CounterpartyRegisterBalances;

public sealed class CounterpartyRegisterBalanceListDtoOrderByBuilder : IOrderByBuilder<CounterpartyRegisterBalance, CounterpartyRegisterBalanceListDto>
{
    public Func<IQueryable<CounterpartyRegisterBalanceListDto>, IOrderedQueryable<CounterpartyRegisterBalanceListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
