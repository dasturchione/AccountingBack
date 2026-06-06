using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountService : ICounterpartyBankAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CounterpartyBankAccount> _query;
    private readonly ICommandRepository<CounterpartyBankAccount> _command;
    private readonly IQueryBuilder<CounterpartyBankAccount> _queryBuilder;

    public CounterpartyBankAccountService(IUserContext userContext, IQueryRepository<CounterpartyBankAccount> query,
        ICommandRepository<CounterpartyBankAccount> command, IQueryBuilder<CounterpartyBankAccount> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CounterpartyBankAccountCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.AccountNumber == dto.AccountNumber, ct))
            return Result.Failure<int>(CounterpartyBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        var entity = new CounterpartyBankAccount
        {
            OrganizationId = dto.OrganizationId,
            CounterpartyId = dto.CounterpartyId,
            BankId = dto.BankId,
            AccountNumber = dto.AccountNumber,
            CurrencyId = dto.CurrencyId,
            IsMain = dto.IsMain,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyBankAccountListDto>>> GetAllAsync(CounterpartyBankAccountListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CounterpartyBankAccountListDto, CounterpartyBankAccountListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CounterpartyBankAccount, CounterpartyBankAccountDto>(id), ct);
        if (entity == null) return Result.Failure<CounterpartyBankAccountDto>(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));

        if (entity.AccountNumber != dto.AccountNumber && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.AccountNumber == dto.AccountNumber, ct))
            return Result.Failure(CounterpartyBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.BankId = dto.BankId;
        entity.AccountNumber = dto.AccountNumber;
        entity.CurrencyId = dto.CurrencyId;
        entity.IsMain = dto.IsMain;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
