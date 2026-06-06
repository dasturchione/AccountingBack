using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountByIdCriteriaBuilder : ICriteriaBuilder<CounterpartyBankAccount, GetByIdOptions<int>>
{
    public Expression<Func<CounterpartyBankAccount, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
