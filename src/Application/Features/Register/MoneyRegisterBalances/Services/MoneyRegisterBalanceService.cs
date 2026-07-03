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
        return Result.Failure<long>(CommonErrors.Forbidden(_userContext.LanguageId));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        return Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));
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
        return Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));
    }
}
