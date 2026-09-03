using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
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
    private readonly IQueryRepository<BankBranch> _bankBranchQuery;
    private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;

    public CounterpartyBankAccountService(IUserContext userContext,
                                          IQueryBuilder queryBuilder, 
                                          IQueryRepository<CounterpartyBankAccount> query,
                                          ICommandRepository<CounterpartyBankAccount> command,
                                          IQueryRepository<BankBranch> bankBranchQuery,
                                          IQueryRepository<CounterpartyCard> counterpartyQuery)
    {
        _query = query;
        _command = command;
        _bankBranchQuery = bankBranchQuery;
        _counterpartyQuery = counterpartyQuery;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(CounterpartyBankAccountCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (await _query.AnyAsync(
                x => x.OrganizationId == organizationId && x.AccountNumber == dto.AccountNumber,
                ct))
            return Result.Failure<int>(CounterpartyBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        if (!await IsBankBranchValidAsync(dto.BankId, dto.BankBranchId, ct))
            return Result.Failure<int>(
                CounterpartyBankAccountErrors.BankBranchMismatch(dto.BankId, dto.BankBranchId!.Value, _userContext.LanguageId));

        if (!await CounterpartyBelongsToOrganizationAsync(dto.CounterpartyId, organizationId, ct))
            return Result.Failure<int>(
                CounterpartyBankAccountErrors.CounterpartyNotFound(dto.CounterpartyId, _userContext.LanguageId));

        var entity = new CounterpartyBankAccount
        {
            OrganizationId = organizationId,
            CounterpartyId = dto.CounterpartyId,
            BankId = dto.BankId,
            BankBranchId = dto.BankBranchId,
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
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyBankAccount>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyBankAccountListDto>>> GetAllAsync(CounterpartyBankAccountListFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<PagedResponse<CounterpartyBankAccountListDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        filter.OrganizationId = organizationId;
        var query = _queryBuilder.BuildPaged<CounterpartyBankAccount, CounterpartyBankAccountListDto, CounterpartyBankAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<CounterpartyBankAccountDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyBankAccount>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .As<CounterpartyBankAccountDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CounterpartyBankAccountDto>(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, CounterpartyBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<CounterpartyBankAccount>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) return Result.Failure(CounterpartyBankAccountErrors.NotFound(id, _userContext.LanguageId));

        if (entity.AccountNumber != dto.AccountNumber &&
            await _query.AnyAsync(
                x => x.OrganizationId == organizationId && x.AccountNumber == dto.AccountNumber,
                ct))
            return Result.Failure(CounterpartyBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        if (!await IsBankBranchValidAsync(dto.BankId, dto.BankBranchId, ct))
            return Result.Failure(
                CounterpartyBankAccountErrors.BankBranchMismatch(dto.BankId, dto.BankBranchId!.Value, _userContext.LanguageId));

        if (!await CounterpartyBelongsToOrganizationAsync(dto.CounterpartyId, organizationId, ct))
            return Result.Failure(
                CounterpartyBankAccountErrors.CounterpartyNotFound(dto.CounterpartyId, _userContext.LanguageId));

        entity.CounterpartyId = dto.CounterpartyId;
        entity.BankId = dto.BankId;
        entity.BankBranchId = dto.BankBranchId;
        entity.AccountNumber = dto.AccountNumber;
        entity.CurrencyId = dto.CurrencyId;
        entity.IsMain = dto.IsMain;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private Task<bool> IsBankBranchValidAsync(int bankId, int? bankBranchId, CancellationToken ct) =>
        !bankBranchId.HasValue
            ? Task.FromResult(true)
            : _bankBranchQuery.AnyAsync(
                x => x.Id == bankBranchId.Value &&
                     x.BankId == bankId &&
                     x.StateId == StateIdConst.ACTIVE,
                ct);

    private Task<bool> CounterpartyBelongsToOrganizationAsync(
        int counterpartyId,
        int organizationId,
        CancellationToken ct) =>
        _counterpartyQuery.AnyAsync(
            counterparty => counterparty.Id == counterpartyId &&
                            counterparty.OrganizationId == organizationId,
            ct);
}
