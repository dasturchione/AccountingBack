using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountListDtoByListFilterCriteriaBuilder : ICriteriaBuilder<OrgBankAccountListDto, OrgBankAccountListFilter>
{
    public Expression<Func<OrgBankAccountListDto, bool>> Build(OrgBankAccountListFilter options)
        => x => string.IsNullOrEmpty(options.Search) ||
                x.AccountNumber.ToLower().Contains(options.Search.ToLower());
}
