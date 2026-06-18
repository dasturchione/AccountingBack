using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.OrgBankAccounts;

public class OrgBankAccountService : IOrgBankAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<BankAccount> _query;
    private readonly ICommandRepository<BankAccount> _command;

    public OrgBankAccountService(IUserContext userContext,
                                 IQueryBuilder queryBuilder, 
                                 IQueryRepository<BankAccount> query,
                                 ICommandRepository<BankAccount> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(OrgBankAccountCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.AccountNumber == dto.AccountNumber, ct))
            return Result.Failure<int>(OrgBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        var entity = new BankAccount
        {
            OrganizationId = orgId,
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
        var query = _queryBuilder.For<BankAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(OrgBankAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<OrgBankAccountListDto>>> GetAllAsync(OrgBankAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<BankAccount, OrgBankAccountListDto, OrgBankAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<OrgBankAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankAccount>().Where(x => x.Id == id).As<OrgBankAccountDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<OrgBankAccountDto>(OrgBankAccountErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, OrgBankAccountUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) return Result.Failure(OrgBankAccountErrors.NotFound(id, _userContext.LanguageId));

        if (entity.AccountNumber != dto.AccountNumber && await _query.AnyAsync(x => x.AccountNumber == dto.AccountNumber, ct))
            return Result.Failure(OrgBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));
        entity.BankId = dto.BankId;
        entity.AccountNumber = dto.AccountNumber;
        entity.CurrencyId = dto.CurrencyId;
        entity.IsMain = dto.IsMain;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
