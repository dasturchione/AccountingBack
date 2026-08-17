using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.CounterpartyBankAccounts;

public sealed class CounterpartyBankAccountListDtoOrderByBuilder : IOrderByBuilder<CounterpartyBankAccount, CounterpartyBankAccountListDto>
{
    public Func<IQueryable<CounterpartyBankAccountListDto>, IOrderedQueryable<CounterpartyBankAccountListDto>> Build() =>
        query => query.OrderByDescending(x => x.IsMain).ThenBy(x => x.CounterpartyName).ThenBy(x => x.Id);
}
