using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.Positions;

public class PositionService : IPositionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Position> _query;
    private readonly ICommandRepository<Position> _command;
    private readonly IQueryBuilder<Position> _queryBuilder;

    public PositionService(IUserContext userContext, IQueryRepository<Position> query,
        ICommandRepository<Position> command, IQueryBuilder<Position> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(PositionCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _query.AnyAsync(p => p.OrganizationId == dto.OrganizationId && p.Code == dto.Code, ct);
        if (exists) return Result.Failure<int>(PositionErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Position { OrganizationId = dto.OrganizationId, Code = dto.Code, Name = dto.Name, StateId = StateIdConst.ACTIVE, CreatedDate = DateTime.Now };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PositionErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PositionListDto>>> GetAllAsync(PositionListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<PositionListDto, PositionListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PositionDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<Position, PositionDto>(id), ct);
        if (entity == null) return Result.Failure<PositionDto>(PositionErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, PositionUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(PositionErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code)
        {
            var exists = await _query.AnyAsync(p => p.OrganizationId == dto.OrganizationId && p.Code == dto.Code, ct);
            if (exists) return Result.Failure(PositionErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        entity.OrganizationId = dto.OrganizationId; entity.Code = dto.Code; entity.Name = dto.Name; entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
