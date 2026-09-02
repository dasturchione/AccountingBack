using Application.Abstractions.Authentication;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountListDtoProjection(IUserContext userContext)
    : IProjectionBuilder<CounterpartyBankAccount, CounterpartyBankAccountListDto>
{
    public Expression<Func<CounterpartyBankAccount, CounterpartyBankAccountListDto>> Build()
    {
        var languageId = userContext.LanguageId;

        return x => new CounterpartyBankAccountListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            CounterpartyId = x.CounterpartyId,
            CounterpartyName = x.Counterparty.ShortName,
            BankId = x.BankId,
            BankBranchId = x.BankBranchId,
            BankName = x.Bank.Name,
            AccountNumber = x.AccountNumber,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.CurrencyTranslations
                .Where(translation => translation.LanguageId == languageId)
                .Select(translation => translation.Name)
                .FirstOrDefault() ?? x.Currency.Name,
            IsMain = x.IsMain,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
    }
}
