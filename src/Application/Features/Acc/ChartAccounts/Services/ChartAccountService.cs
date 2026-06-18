using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public class ChartAccountService : IChartAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<ChartAccount> _query;
    private readonly ICommandRepository<ChartAccount> _command;

    public ChartAccountService(IUserContext userContext,
                               IQueryBuilder queryBuilder, 
                               IQueryRepository<ChartAccount> query, 
                               ICommandRepository<ChartAccount> command)
    {
        _query = query; 
        _command = command; 
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ChartAccountCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        if (await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure<int>(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new ChartAccount
        {
            OrganizationId = orgId,
            ParentId = dto.ParentId,
            Code = dto.Code,
            Name = dto.Name,
            IsGroup = dto.IsGroup,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ChartAccountListDto>>> GetAllAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<ChartAccount, ChartAccountListDto, ChartAccountListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ChartAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).As<ChartAccountDto>().Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure<ChartAccountDto>(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ChartAccountUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<ChartAccount>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.Code == dto.Code, ct))
            return Result.Failure(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        entity.ParentId = dto.ParentId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.IsGroup = dto.IsGroup;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
