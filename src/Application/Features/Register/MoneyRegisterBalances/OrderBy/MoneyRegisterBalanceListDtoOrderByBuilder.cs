using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.MoneyRegisterBalances;

public sealed class MoneyRegisterBalanceListDtoOrderByBuilder : IOrderByBuilder<MoneyRegisterBalance, MoneyRegisterBalanceListDto>
{
    public Func<IQueryable<MoneyRegisterBalanceListDto>, IOrderedQueryable<MoneyRegisterBalanceListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
