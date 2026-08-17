using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.BankTerminals;

public class BankTerminalService : IBankTerminalService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<BankTerminal> _query;
    private readonly IQueryRepository<BankAccount> _bankAccountQuery;
    private readonly ICommandRepository<BankTerminal> _command;

    public BankTerminalService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<BankTerminal> query,
        IQueryRepository<BankAccount> bankAccountQuery,
        ICommandRepository<BankTerminal> command)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _bankAccountQuery = bankAccountQuery;
        _command = command;
    }

    public async Task<Result<int>> CreateAsync(BankTerminalCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId)
            return Result.Failure<int>(BankTerminalErrors.OrganizationRequired(_userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, organizationId, null, ct);
        if (!validationResult.IsSuccess)
            return Result.Failure<int>(validationResult.Error);

        var entity = new BankTerminal
        {
            OrganizationId = organizationId,
            BankAccountId = dto.BankAccountId,
            Name = dto.Name,
            MerchantId = NormalizeOptional(dto.MerchantId),
            ExternalTerminalId = NormalizeOptional(dto.ExternalTerminalId),
            SerialNumber = NormalizeOptional(dto.SerialNumber),
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(BankTerminalErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<BankTerminalListDto>>> GetAllAsync(BankTerminalListFilter filter, CancellationToken ct = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = filter.PageSize.GetValueOrDefault(50);
        var search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim().ToLower();

        var query = _queryBuilder.For<BankTerminal>()
            .Where(x =>
                (!filter.BankAccountId.HasValue || x.BankAccountId == filter.BankAccountId.Value) &&
                (!filter.StateId.HasValue || x.StateId == filter.StateId.Value))
            .As(ToListDto())
            .Where(x => search == null ||
                x.Name.ToLower().Contains(search) ||
                (x.MerchantId != null && x.MerchantId.ToLower().Contains(search)) ||
                (x.ExternalTerminalId != null && x.ExternalTerminalId.ToLower().Contains(search)) ||
                (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(search)) ||
                (x.BankAccountNumber != null && x.BankAccountNumber.ToLower().Contains(search)))
            .OrderBy(x => x.OrderBy(t => t.Name).ThenBy(t => t.Id))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .BuildPaged();

        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, page, pageSize);
    }

    public async Task<Result<BankTerminalDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<BankTerminal>()
            .Where(x => x.Id == id)
            .As(ToDto())
            .Build();

        var terminal = await _query.GetAsync(query, ct);
        return terminal is null
            ? Result.Failure<BankTerminalDto>(BankTerminalErrors.NotFound(id, _userContext.LanguageId))
            : terminal;
    }

    public async Task<Result> UpdateAsync(int id, BankTerminalUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await GetEntityAsync(id, ct);
        if (entity is null)
            return Result.Failure(BankTerminalErrors.NotFound(id, _userContext.LanguageId));

        var validationResult = await ValidateReferencesAndUniquenessAsync(dto, entity.OrganizationId, id, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        entity.BankAccountId = dto.BankAccountId;
        entity.Name = dto.Name;
        entity.MerchantId = NormalizeOptional(dto.MerchantId);
        entity.ExternalTerminalId = NormalizeOptional(dto.ExternalTerminalId);
        entity.SerialNumber = NormalizeOptional(dto.SerialNumber);
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<BankTerminal?> GetEntityAsync(int id, CancellationToken ct)
    {
        var query = _queryBuilder.For<BankTerminal>()
            .Where(x => x.Id == id)
            .Build();

        return await _query.GetAsync(query, ct);
    }

    private async Task<Result> ValidateReferencesAndUniquenessAsync(
        BankTerminalBaseDto dto,
        int organizationId,
        int? excludedId,
        CancellationToken ct)
    {
        if (dto.BankAccountId.HasValue && !await _bankAccountQuery.AnyAsync(
                x => x.Id == dto.BankAccountId.Value && x.OrganizationId == organizationId,
                ct))
        {
            return Result.Failure(BankTerminalErrors.BankAccountNotFound(dto.BankAccountId.Value, _userContext.LanguageId));
        }

        var externalTerminalId = NormalizeOptional(dto.ExternalTerminalId);
        if (externalTerminalId is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.ExternalTerminalId == externalTerminalId &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(BankTerminalErrors.ExternalTerminalIdConflict(externalTerminalId, _userContext.LanguageId));
        }

        var serialNumber = NormalizeOptional(dto.SerialNumber);
        if (serialNumber is not null && await _query.AnyAsync(
                x => x.OrganizationId == organizationId &&
                     x.SerialNumber == serialNumber &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                ct))
        {
            return Result.Failure(BankTerminalErrors.SerialNumberConflict(serialNumber, _userContext.LanguageId));
        }

        return Result.Success();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static System.Linq.Expressions.Expression<Func<BankTerminal, BankTerminalDto>> ToDto() =>
        x => new BankTerminalDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount != null ? x.BankAccount.AccountNumber : null,
            Name = x.Name,
            MerchantId = x.MerchantId,
            ExternalTerminalId = x.ExternalTerminalId,
            SerialNumber = x.SerialNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };

    private static System.Linq.Expressions.Expression<Func<BankTerminal, BankTerminalListDto>> ToListDto() =>
        x => new BankTerminalListDto
        {
            Id = x.Id,
            OrganizationId = x.OrganizationId,
            OrganizationName = x.Organization.ShortName,
            BankAccountId = x.BankAccountId,
            BankAccountNumber = x.BankAccount != null ? x.BankAccount.AccountNumber : null,
            Name = x.Name,
            MerchantId = x.MerchantId,
            ExternalTerminalId = x.ExternalTerminalId,
            SerialNumber = x.SerialNumber,
            StateId = x.StateId,
            StateName = x.State.FullName,
            CreatedDate = x.CreatedDate
        };
}
