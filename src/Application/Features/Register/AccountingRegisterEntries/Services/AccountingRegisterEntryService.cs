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
    private readonly IQueryRepository<PostingRule> _postingRuleQuery;
    private readonly IQueryRepository<AccountingRegisterEntry> _query;
    private readonly ICommandRepository<AccountingRegisterEntry> _command;

    public AccountingRegisterEntryService(IUserContext userContext,
                                          IQueryBuilder queryBuilder,
                                          IQueryRepository<PostingRule> postingRuleQuery,
                                          IQueryRepository<AccountingRegisterEntry> query,
                                          ICommandRepository<AccountingRegisterEntry> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _postingRuleQuery = postingRuleQuery;
    }

    public async Task<Result<long>> CreateAsync(AccountingRegisterEntryCreateDto dto, CancellationToken ct = default)
    {
        var entity = new AccountingRegisterEntry
        {
            OrganizationId = dto.OrganizationId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentId = dto.DocumentId,
            DebitAccountId = dto.DebitAccountId,
            CreditAccountId = dto.CreditAccountId,
            CurrencyId = dto.CurrencyId,
            Amount = dto.Amount,
            DocDate = dto.DocDate,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));

        //await _command.DeleteAsync(entity, ct);
        return Result.Success();
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
        //if (!items.Any())
        return items;
    }

    public async Task<Result<List<AccountingPostingDto>>> GetDailyPostingAsync(DateTime startDate, DateTime endDate, short? documentTypeId, CancellationToken ct = default)
    {
        var endOfDay = endDate.Date.AddDays(1).AddTicks(-1);
        var queryBuilder = _queryBuilder.For<AccountingRegisterEntry>()
            .Where(x => x.DocDate >= startDate.Date && x.DocDate <= endOfDay);

        if (documentTypeId.HasValue)
            queryBuilder = queryBuilder.Where(x => x.DocumentTypeId == documentTypeId.Value);

        var query = queryBuilder.As<AccountingPostingDto>().Build();
        var items = await _query.GetAllAsync(query, ct);

        return items;
    }

    public async Task<Result> UpdateAsync(long id, AccountingRegisterEntryUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<AccountingRegisterEntry>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.DocumentTypeId = dto.DocumentTypeId;
        entity.DocumentId = dto.DocumentId;
        entity.DebitAccountId = dto.DebitAccountId;
        entity.CreditAccountId = dto.CreditAccountId;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.DocDate = dto.DocDate;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
