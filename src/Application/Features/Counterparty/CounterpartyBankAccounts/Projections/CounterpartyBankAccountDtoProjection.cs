using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountDtoProjection : IProjectionBuilder<CounterpartyBankAccount, CounterpartyBankAccountDto>
{
    public Expression<Func<CounterpartyBankAccount, CounterpartyBankAccountDto>> Build() =>
        x => new CounterpartyBankAccountDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            BankId = x.BankId,
            BankName = x.Bank.Name,
            AccountNumber = x.AccountNumber,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            IsMain = x.IsMain,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
