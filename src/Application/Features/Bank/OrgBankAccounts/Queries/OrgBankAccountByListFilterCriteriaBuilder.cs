using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountByListFilterCriteriaBuilder : ICriteriaBuilder<OrgBankAccount, OrgBankAccountListFilter>
{
    public Expression<Func<OrgBankAccount, bool>> Build(OrgBankAccountListFilter options) =>
        x => (!options.OrganizationId.HasValue || x.OrganizationId == options.OrganizationId.Value) &&
             (!options.BankId.HasValue || x.BankId == options.BankId.Value);
}
