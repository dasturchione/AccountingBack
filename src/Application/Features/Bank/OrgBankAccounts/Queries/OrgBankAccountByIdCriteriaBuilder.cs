using Application.Options;
using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountByIdCriteriaBuilder : ICriteriaBuilder<OrgBankAccount, GetByIdOptions<int>>
{
    public Expression<Func<OrgBankAccount, bool>> Build(GetByIdOptions<int> options) => x => x.Id == options.Id;
}
