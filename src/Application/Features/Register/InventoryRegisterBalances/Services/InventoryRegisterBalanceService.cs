using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.InventoryRegisterBalances;

public class InventoryRegisterBalanceService : IInventoryRegisterBalanceService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<InventoryRegisterBalance> _query;

    public InventoryRegisterBalanceService(IUserContext userContext,
                                           IQueryBuilder queryBuilder,
                                           IQueryRepository<InventoryRegisterBalance> query)
    {
        _query = query;
        _userContext = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<PagedResponse<InventoryRegisterBalanceListDto>>> GetAllAsync(InventoryRegisterBalanceListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<InventoryRegisterBalance, InventoryRegisterBalanceListDto, InventoryRegisterBalanceListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<InventoryRegisterBalanceDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<InventoryRegisterBalance>().Where(x => x.Id == id).As<InventoryRegisterBalanceDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null)
            return Result.Failure<InventoryRegisterBalanceDto>(InventoryRegisterBalanceErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }
}
