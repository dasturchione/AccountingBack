using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class CounterpartyRegisterBalanceService : ICounterpartyRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;
    private readonly IQueryBuilder<CounterpartyRegisterBalance> _queryBuilder;

    public CounterpartyRegisterBalanceService(
        IUserContext userContext,
        IQueryRepository<CounterpartyRegisterBalance> query,
        ICommandRepository<CounterpartyRegisterBalance> command,
        IQueryBuilder<CounterpartyRegisterBalance> queryBuilder)
    {
        _userContext = userContext;
        _query = query;
        _command = command;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(CounterpartyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        var entity = new CounterpartyRegisterBalance
        {
            OrganizationId = dto.OrganizationId,
            DocumentTypeId = dto.DocumentTypeId,
            DocumentId = dto.DocumentId,
            CounterpartyId = dto.CounterpartyId,
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
        if (entity == null) return Result.Failure(CounterpartyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<CounterpartyRegisterBalanceListDto>>> GetAllAsync(CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<CounterpartyRegisterBalanceListDto, CounterpartyRegisterBalanceListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<CounterpartyRegisterBalance, CounterpartyRegisterBalanceDto>(id), ct);
        if (entity == null) return Result.Failure<CounterpartyRegisterBalanceDto>(CounterpartyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(long id, CounterpartyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(CounterpartyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.DocumentTypeId = dto.DocumentTypeId;
        entity.DocumentId = dto.DocumentId;
        entity.CounterpartyId = dto.CounterpartyId;
        entity.OperationTypeId = dto.OperationTypeId;
        entity.CurrencyId = dto.CurrencyId;
        entity.Amount = dto.Amount;
        entity.DocDate = dto.DocDate;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
