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
    private readonly IQueryRepository<Organization> _organizationQuery;
    private readonly ICommandRepository<BankAccount> _command;

    public OrgBankAccountService(IUserContext userContext,
                                 IQueryBuilder queryBuilder, 
                                 IQueryRepository<BankAccount> query,
                                 IQueryRepository<Organization> organizationQuery,
                                 ICommandRepository<BankAccount> command)
    {
        _query = query;
        _organizationQuery = organizationQuery;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<OrgBankAccountCreateResultDto>> CreateAsync(OrgBankAccountCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.AccountNumber == dto.AccountNumber, ct))
            return Result.Failure<OrgBankAccountCreateResultDto>(OrgBankAccountErrors.AccountNumberConflict(dto.AccountNumber, _userContext.LanguageId));

        var entity = BuildCreateEntity(dto, orgId);
        await _command.CreateAsync(entity, ct);
        var inn = await GetOrganizationInnAsync(orgId, ct);

        return ToCreateResult(entity, inn);
    }

    public async Task<Result<List<OrgBankAccountCreateResultDto>>> CreateManyAsync(OrgBankAccountCreateManyDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;
        var duplicateAccountNumber = dto.Accounts
            .GroupBy(x => x.AccountNumber)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateAccountNumber != null)
            return Result.Failure<List<OrgBankAccountCreateResultDto>>(OrgBankAccountErrors.AccountNumberConflict(duplicateAccountNumber, _userContext.LanguageId));

        foreach (var account in dto.Accounts)
        {
            if (await _query.AnyAsync(x => x.AccountNumber == account.AccountNumber, ct))
                return Result.Failure<List<OrgBankAccountCreateResultDto>>(OrgBankAccountErrors.AccountNumberConflict(account.AccountNumber, _userContext.LanguageId));
        }

        var entities = dto.Accounts.Select(account => BuildCreateEntity(account, orgId)).ToList();

        await _command.CreateAsync(entities, ct);

        var inn = await GetOrganizationInnAsync(orgId, ct);
        return entities.Select(entity => ToCreateResult(entity, inn)).ToList();
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

    private static BankAccount BuildCreateEntity(OrgBankAccountCreateDto dto, int orgId) =>
        new()
        {
            OrganizationId = orgId,
            BankId = dto.BankId,
            AccountNumber = dto.AccountNumber,
            CurrencyId = dto.CurrencyId,
            IsMain = dto.IsMain,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

    private async Task<string> GetOrganizationInnAsync(int orgId, CancellationToken ct)
    {
        var query = _queryBuilder.For<Organization>().Where(x => x.Id == orgId).Build();
        var organization = await _organizationQuery.GetAsync(query, ct);
        return organization?.Inn ?? string.Empty;
    }

    private static OrgBankAccountCreateResultDto ToCreateResult(BankAccount entity, string inn) =>
        new()
        {
            Id = entity.Id,
            Inn = inn,
            AccountNumber = entity.AccountNumber
        };
}
