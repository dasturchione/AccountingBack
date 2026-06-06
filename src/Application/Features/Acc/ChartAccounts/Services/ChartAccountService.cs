using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.ChartAccounts;

public class ChartAccountService : IChartAccountService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<ChartAccount> _query;
    private readonly ICommandRepository<ChartAccount> _command;
    private readonly IQueryBuilder<ChartAccount> _queryBuilder;

    public ChartAccountService(IUserContext userContext, IQueryRepository<ChartAccount> query,
        ICommandRepository<ChartAccount> command, IQueryBuilder<ChartAccount> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(ChartAccountCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure<int>(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new ChartAccount
        {
            OrganizationId = dto.OrganizationId,
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
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<ChartAccountListDto>>> GetAllAsync(ChartAccountListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<ChartAccountListDto, ChartAccountListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<ChartAccountDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<ChartAccount, ChartAccountDto>(id), ct);
        if (entity == null) return Result.Failure<ChartAccountDto>(ChartAccountErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, ChartAccountUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(ChartAccountErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure(ChartAccountErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.ParentId = dto.ParentId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.IsGroup = dto.IsGroup;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
