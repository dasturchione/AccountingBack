using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.AccountingRegisterEntries;

public class AccountingRegisterEntryService : IAccountingRegisterEntryService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<AccountingRegisterEntry> _query;
    private readonly ICommandRepository<AccountingRegisterEntry> _command;
    private readonly IQueryBuilder<AccountingRegisterEntry> _queryBuilder;

    public AccountingRegisterEntryService(
        IUserContext userContext,
        IQueryRepository<AccountingRegisterEntry> query,
        ICommandRepository<AccountingRegisterEntry> command,
        IQueryBuilder<AccountingRegisterEntry> queryBuilder)
    {
        _userContext = userContext;
        _query = query;
        _command = command;
        _queryBuilder = queryBuilder;
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));

        await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<AccountingRegisterEntryListDto>>> GetAllAsync(AccountingRegisterEntryListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<AccountingRegisterEntryListDto, AccountingRegisterEntryListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<AccountingRegisterEntryDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<AccountingRegisterEntry, AccountingRegisterEntryDto>(id), ct);
        if (entity == null) return Result.Failure<AccountingRegisterEntryDto>(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(long id, AccountingRegisterEntryUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(AccountingRegisterEntryErrors.NotFound(id, _userContext.LanguageId));

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
