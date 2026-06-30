using Application.Features.Register.AccountingRegisterEntries;
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
            PostingBatchId = x.PostingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.ReversalEntryId,
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
            PostingBatchId = x.PostingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.ReversalEntryId,
            CreatedDate = x.CreatedDate
        };
}

public class AccountingPostingDtoProjection : IProjectionBuilder<AccountingRegisterEntry, AccountingPostingDto>
{
    public Expression<Func<AccountingRegisterEntry, AccountingPostingDto>> Build() =>
        x => new AccountingPostingDto
        {
            Id = x.Id, 
            OrganizationId = x.OrganizationId,
            DebitAccountId = x.DebitAccountId,
            CreditAccountId = x.CreditAccountId,
            CreatedDate = x.CreatedDate,
            Amount = x.Amount,
            CurrencyId = x.CurrencyId,
            DocDate = x.DocDate, 
            DocumentId = x.DocumentId,
            DocumentTypeId = x.DocumentTypeId,
            PostingBatchId = x.PostingBatchId,
            SourceLineId = x.SourceLineId,
            ReversalEntryId = x.ReversalEntryId,
            CreditAccountCode = x.CreditAccount != null ? x.CreditAccount.Code : null,
            CreditAccountName = x.CreditAccount != null ? x.CreditAccount.Name : null,
            DebitAccountCode = x.DebitAccount != null ? x.DebitAccount.Code : null,
            DebitAccountName = x.DebitAccount != null ? x.DebitAccount.Name : null,
            CurrencyCode = x.Currency.Code,
            CurrencyName = x.Currency.Name,
            DebitQuantity = x.DebitQuantity,
            CreditQuantity = x.CreditQuantity,
            Tables = x.RegisterEntrySubkontos.OrderBy(o => o.SortOrder).Select(s => new AccountingPostingTableDto
            {
                Id = s.Id,
                EntityId = s.EntityId,
                SubkontoTypeId = s.SubkontoTypeId,
                SubkontoTypeName = s.SubkontoType.Name,
                SubkontoTypeCode = s.SubkontoType.Code,
                Side = s.Side,
                CreatedDate = s.CreatedDate,
                EntryId = s.EntryId,
                DisplayValue = s.DisplayValue,
                SortOrder = s.SortOrder
            }).ToList()
        };
}
