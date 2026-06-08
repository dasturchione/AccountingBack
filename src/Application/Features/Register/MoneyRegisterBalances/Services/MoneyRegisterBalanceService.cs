using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.MoneyRegisterBalances;

public class MoneyRegisterBalanceService : IMoneyRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<MoneyRegisterBalance> _query;
    private readonly ICommandRepository<MoneyRegisterBalance> _command;

    public MoneyRegisterBalanceService(IUserContext userContext,
                                       IQueryBuilder queryBuilder,
                                       IQueryRepository<MoneyRegisterBalance> query,
                                       ICommandRepository<MoneyRegisterBalance> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
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
        var query = _queryBuilder.For<MoneyRegisterBalance>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

        //await _command.DeleteAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<MoneyRegisterBalanceListDto>>> GetAllAsync(MoneyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<MoneyRegisterBalance, MoneyRegisterBalanceListDto, MoneyRegisterBalanceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<MoneyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>().Where(x => x.Id == id).As<MoneyRegisterBalanceDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<MoneyRegisterBalanceDto>(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, MoneyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<MoneyRegisterBalance>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(MoneyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));

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
