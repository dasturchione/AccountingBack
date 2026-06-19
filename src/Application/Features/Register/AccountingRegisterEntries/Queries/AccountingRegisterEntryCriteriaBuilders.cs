using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryByListFilterCriteriaBuilder : ICriteriaBuilder<AccountingRegisterEntry, AccountingRegisterEntryListFilter>
{
    public Expression<Func<AccountingRegisterEntry, bool>> Build(AccountingRegisterEntryListFilter options) =>
        x => (!options.DocumentTypeId.HasValue || x.DocumentTypeId == options.DocumentTypeId.Value) &&
             (!options.DocumentId.HasValue || x.DocumentId == options.DocumentId.Value) &&
             (!options.DebitAccountId.HasValue || x.DebitAccountId == options.DebitAccountId.Value) &&
             (!options.CreditAccountId.HasValue || x.CreditAccountId == options.CreditAccountId.Value) &&
             (!options.CurrencyId.HasValue || x.CurrencyId == options.CurrencyId.Value) &&
             (options.DateFrom == null || x.DocDate >= options.DateFrom) &&
             (options.DateTo == null || x.DocDate <= options.DateTo);
}
