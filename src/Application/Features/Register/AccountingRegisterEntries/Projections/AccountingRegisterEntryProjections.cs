using Domain.Entities;
using SharedKernel.Query;
using System.Linq.Expressions;

namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryDtoProjection : IProjectionBuilder<AccountingRegisterEntry, AccountingRegisterEntryDto>
{
    public Expression<Func<AccountingRegisterEntry, AccountingRegisterEntryDto>> Build() =>
        x => new AccountingRegisterEntryDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            DebitAccountId = x.DebitAccountId,
            CreditAccountId = x.CreditAccountId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}

public class AccountingRegisterEntryListDtoProjection : IProjectionBuilder<AccountingRegisterEntry, AccountingRegisterEntryListDto>
{
    public Expression<Func<AccountingRegisterEntry, AccountingRegisterEntryListDto>> Build() =>
        x => new AccountingRegisterEntryListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            DocumentTypeId = x.DocumentTypeId,
            DocumentId = x.DocumentId,
            DebitAccountId = x.DebitAccountId,
            CreditAccountId = x.CreditAccountId,
            CurrencyId = x.CurrencyId,
            Amount = x.Amount,
            DocDate = x.DocDate,
            CreatedDate = x.CreatedDate
        };
}
