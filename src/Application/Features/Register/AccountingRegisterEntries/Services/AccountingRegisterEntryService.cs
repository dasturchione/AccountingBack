using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Register.AccountingRegisterEntries;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryService : IAccountingRegisterEntryService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<AccountingRegisterEntry> _query;

    public AccountingRegisterEntryService(IUserContext userContext,
                                          IQueryBuilder queryBuilder,
                                          IQueryRepository<AccountingRegisterEntry> query)
    {
        _query = query;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<PagedResponse<AccountingRegisterEntryListDto>>> GetAllAsync(AccountingRegisterEntryListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<AccountingRegisterEntry, AccountingRegisterEntryListDto, AccountingRegisterEntryListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<AccountingRegisterEntryDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>().Where(x => x.Id == id).As<AccountingRegisterEntryDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<AccountingRegisterEntryDto>(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result<List<AccountingPostingDto>>> GetPostingAsync(short documentTypeId, long documentId, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>()
                                    .Where(x => x.DocumentTypeId == documentTypeId && x.DocumentId == documentId)
                                    .As<AccountingPostingDto>()
                                    .Build();
        var items = await _query.GetAllAsync(query, ct);
        return items;
    }

    public async Task<Result<List<AccountingPostingDto>>> GetDailyPostingAsync(DateTime startDate, DateTime endDate, short? documentTypeId, CancellationToken ct = default)
    {
        var endOfDay = endDate.Date.AddDays(1).AddTicks(-1);

        var query = _queryBuilder.For<AccountingRegisterEntry>()
                        .Where(x => x.DocDate >= startDate.Date && 
                                    x.DocDate <= endOfDay && 
                                    (documentTypeId == null || 
                                     x.DocumentTypeId == documentTypeId.Value))
                        .As<AccountingPostingDto>()
                        .OrderBy(o => o.DocDate).Desc()
                        .Build();
        
        var items = await _query.GetAllAsync(query, ct);

        return items;
    }

}
