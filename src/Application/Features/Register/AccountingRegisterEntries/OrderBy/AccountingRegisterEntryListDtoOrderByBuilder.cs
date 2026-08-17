using Domain.Entities;
using SharedKernel.Query;

namespace Application.Features.AccountingRegisterEntries;

public sealed class AccountingRegisterEntryListDtoOrderByBuilder : IOrderByBuilder<AccountingRegisterEntry, AccountingRegisterEntryListDto>
{
    public Func<IQueryable<AccountingRegisterEntryListDto>, IOrderedQueryable<AccountingRegisterEntryListDto>> Build() =>
        query => query.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.Id);
}
