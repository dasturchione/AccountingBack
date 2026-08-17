using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.OrgBankAccounts;

public sealed class OrgBankAccountListDtoOrderByBuilder : IOrderByBuilder<BankAccount, OrgBankAccountListDto>
{
    public Func<IQueryable<OrgBankAccountListDto>, IOrderedQueryable<OrgBankAccountListDto>> Build() =>
        query => query.OrderByDescending(x => x.IsMain).ThenBy(x => x.Name).ThenBy(x => x.Id);
}
