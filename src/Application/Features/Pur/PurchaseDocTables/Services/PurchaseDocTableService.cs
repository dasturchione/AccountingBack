using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.PurchaseDocTables;

public class PurchaseDocTableService : IPurchaseDocTableService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<PurchaseDocTable> _query;

    public PurchaseDocTableService(IUserContext userContext,
                                   IQueryBuilder queryBuilder,
                                   IQueryRepository<PurchaseDocTable> query)
    {
        _query        = query;
        _userContext  = userContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<PagedResponse<PurchaseDocTableListDto>>> GetAllAsync(PurchaseDocTableListFilter filter, CancellationToken ct = default)
    {
        var query     = _queryBuilder.BuildPaged<PurchaseDocTable, PurchaseDocTableListDto, PurchaseDocTableListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PurchaseDocTableDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var query  = _queryBuilder.For<PurchaseDocTable>().Where(e => e.Id == id).As<PurchaseDocTableDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<PurchaseDocTableDto>(PurchaseDocTableErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<long>> CreateAsync(PurchaseDocTableCreateDto dto, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure<long>(PurchaseDocTableErrors.DirectTableCreateUnsupported(_userContext.LanguageId));
    }

    public async Task<Result> UpdateAsync(long id, PurchaseDocTableUpdateDto dto, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure(PurchaseDocTableErrors.DirectTableMutationUnsupported(_userContext.LanguageId));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return Result.Failure(PurchaseDocTableErrors.DirectTableMutationUnsupported(_userContext.LanguageId));
    }
}
