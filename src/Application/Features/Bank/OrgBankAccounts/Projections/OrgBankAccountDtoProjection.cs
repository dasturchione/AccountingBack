using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountDtoProjection : IProjectionBuilder<BankAccount, OrgBankAccountDto>
{
    public Expression<Func<BankAccount, OrgBankAccountDto>> Build() =>
        x => new OrgBankAccountDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            Inn = x.Organization.Inn,
            BankId = x.BankId,
            Code = x.Code,
            Name = x.Name,
            BankName = x.Bank.Name,
            AccountNumber = x.AccountNumber,
            CurrencyId = x.CurrencyId,
            CurrencyName = x.Currency.Name,
            IsMain = x.IsMain,
            OpeningBalance = x.OpeningBalance,
            OpeningBalanceDate = x.OpeningBalanceDate,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
