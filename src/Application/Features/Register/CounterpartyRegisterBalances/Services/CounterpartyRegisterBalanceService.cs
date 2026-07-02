using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.CounterpartyRegisterBalances;

public class CounterpartyRegisterBalanceService : ICounterpartyRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<CounterpartyRegisterBalance> _query;
    private readonly ICommandRepository<CounterpartyRegisterBalance> _command;

    public CounterpartyRegisterBalanceService(IUserContext userContext,
                                              IQueryBuilder queryBuilder,
                                              IQueryRepository<CounterpartyRegisterBalance> query,
                                              ICommandRepository<CounterpartyRegisterBalance> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<long>> CreateAsync(CounterpartyRegisterBalanceCreateDto dto, CancellationToken ct = default)
    {
        return Result.Failure<long>(CommonErrors.Forbidden(_userContext.LanguageId));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        return Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));
    }

    public async Task<Result<PagedResponse<CounterpartyRegisterBalanceListDto>>> GetAllAsync(CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<CounterpartyRegisterBalance, CounterpartyRegisterBalanceListDto, CounterpartyRegisterBalanceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<CounterpartyRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<CounterpartyRegisterBalance>().Where(x => x.Id == id).As<CounterpartyRegisterBalanceDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<CounterpartyRegisterBalanceDto>(CounterpartyRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(long id, CounterpartyRegisterBalanceUpdateDto dto, CancellationToken ct = default)
    {
        return Result.Failure(CommonErrors.Forbidden(_userContext.LanguageId));
    }
}
