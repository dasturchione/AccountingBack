using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyBankAccountListDto, CounterpartyBankAccountListFilter>
{
    public Expression<Func<CounterpartyBankAccountListDto, bool>> Build(CounterpartyBankAccountListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.AccountNumber.ToLower().Contains(options.Search.ToLower());
}
