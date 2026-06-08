using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyBankAccounts;

public class CounterpartyBankAccountService : ICounterpartyBankAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyBankAccount> _query;
    private readonly ICommandRepository<CounterpartyBankAccount> _command;

    public CounterpartyBankAccountService(IUserContext userContext,
                                          IQueryBuilder queryBuilder, 
                                          IQueryRepository<CounterpartyBankAccount> query,
                                          ICommandRepository<CounterpartyBankAccount> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
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
        var query = _queryBuilder.For<CounterpartyBankAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyBankAccountListDto>>> GetAllAsync(CounterpartyBankAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<CounterpartyBankAccount, CounterpartyBankAccountListDto, CounterpartyBankAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyBankAccount>().Where(x => x.Id == id).As<CounterpartyBankAccountDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CounterpartyBankAccountDto>(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyBankAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
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
