using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceService : IMoneyRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;
    private readonly IQueryBuilder<MoneyRegisterBalance> _queryBuilder;

    public MoneyRegisterBalanceService(
        IUserContext userContext,
        IQueryRepository<MoneyRegisterBalance> query,
        ICommandRepository<MoneyRegisterBalance> command,
        IQueryBuilder<MoneyRegisterBalance> queryBuilder)
    {
        _userContext = userContext;
        _query = query;
        _command = command;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(MoneyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        var entity = new MoneyRegisterBalance
        {
            OrganizationId = dto.OrganizationId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentId = dto.DocumentId,
            SourceType = dto.SourceType,
            SourceId = dto.SourceId,
            OperationTypeId = dto.OperationTypeId,
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
        if (entity == null) return Result.Failure(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<MoneyRegisterBalanceListDto>>> GetAllAsync(MoneyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<MoneyRegisterBalanceListDto, MoneyRegisterBalanceListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<MoneyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<MoneyRegisterBalance, MoneyRegisterBalanceDto>(id), ct);
        if (entity == null) return Result.Failure<MoneyRegisterBalanceDto>(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(long id, MoneyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.DocumentTypeId = dto.DocumentTypeId;
        entity.DocumentId = dto.DocumentId;
        entity.SourceType = dto.SourceType;
        entity.SourceId = dto.SourceId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.DocDate = dto.DocDate;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
