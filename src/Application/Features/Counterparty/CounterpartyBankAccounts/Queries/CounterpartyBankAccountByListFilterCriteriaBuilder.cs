using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountByListFilterCriteriaBuilder : ICriteriaBuilder<CounterpartyBankAccount, CounterpartyBankAccountListFilter>
{
    public Expression<Func<CounterpartyBankAccount, bool>> Build(CounterpartyBankAccountListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.CounterpartyId.HasValue || x.CounterpartyId == options.CounterpartyId.Value);
}
