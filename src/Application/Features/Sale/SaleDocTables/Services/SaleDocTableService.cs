using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.SaleDocTables;

public class SaleDocTableService : ISaleDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<SaleDocTable> _query;

    public SaleDocTableService(IUserContext userContext,
                               IQueryBuilder queryBuilder,
                               IQueryRepository<SaleDocTable> query)
    {
        _query              = query;
        _userContext        = userContext;
        _queryBuilder       = queryBuilder;
    }

    public async Task<Result<PagedResponse<SaleDocTableListDto>>> GetAllAsync(SaleDocTableListFilter filter, CancellationToken ct = default)
    {
        var query     = _queryBuilder.BuildPaged<SaleDocTable, SaleDocTableListDto, SaleDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<SaleDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query  = _queryBuilder.For<SaleDocTable>().Where(e => e.Id == id).As<SaleDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<SaleDocTableDto>(SaleDocTableErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(SaleDocTableCreateDto dto, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure<long>(SaleDocTableErrors.DirectTableMutationUnsupported(_userContext.LanguageId));
    }

    public async Task<Result> UpdateAsync(long id, SaleDocTableUpdateDto dto, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure(SaleDocTableErrors.DirectTableMutationUnsupported(_userContext.LanguageId));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure(SaleDocTableErrors.DirectTableMutationUnsupported(_userContext.LanguageId));
    }
}
