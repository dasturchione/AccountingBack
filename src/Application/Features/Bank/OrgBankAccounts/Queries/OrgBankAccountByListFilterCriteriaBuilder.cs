using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountByListFilterCriteriaBuilder : ICriteriaBuilder<BankAccount, OrgBankAccountListFilter>
{
    public Expression<Func<BankAccount, bool>> Build(OrgBankAccountListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.BankId.HasValue || x.BankId == options.BankId.Value);
}
