using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Banks;

public class BankService : IBankService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Bank> _query;
    private readonly IQueryRepository<BankBranch> _bankBranchQuery;

    public BankService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<Bank> query,
        IQueryRepository<BankBranch> bankBranchQuery)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _query = query;
        _bankBranchQuery = bankBranchQuery;
    }

    public async Task<Result<PagedResponse<BankListDto>>> GetAllAsync(BankListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Bank, BankListDto, BankListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<BankDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Bank>().Where(x => x.Id == id).As<BankDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null)
            return Result.Failure<BankDto>(BankErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result<List<BankBranchDto>>> GetBranchesAsync(int bankId, CancellationToken ct = default)
    {
        if (!await _query.AnyAsync(x => x.Id == bankId, ct))
            return Result.Failure<List<BankBranchDto>>(BankErrors.NotFound(bankId, _userContext.LanguageId));

        var query = _queryBuilder.For<BankBranch>()
            .Where(x => x.BankId == bankId)
            .As<BankBranchDto>()
            .OrderBy(x => x.Name)
            .Build();

        return await _bankBranchQuery.GetAllAsync(query, ct);
    }

    public async Task<Result<BankBranchDto>> GetBranchByMfoAsync(string mfo, CancellationToken ct = default)
    {
        var normalizedMfo = mfo.Trim();
        var query = _queryBuilder.For<BankBranch>()
            .Where(x => x.Mfo == normalizedMfo)
            .As<BankBranchDto>()
            .Build();
        var branch = await _bankBranchQuery.GetAsync(query, ct);

        if (branch == null)
            return Result.Failure<BankBranchDto>(BankErrors.BranchNotFound(normalizedMfo, _userContext.LanguageId));

        return branch;
    }
}
