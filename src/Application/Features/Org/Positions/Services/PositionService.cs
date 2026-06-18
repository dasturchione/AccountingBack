using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Positions;

public class PositionService : IPositionService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Position> _query;
    private readonly ICommandRepository<Position> _command;

    public PositionService(IUserContext userContext,
                           IQueryBuilder queryBuilder, 
                           IQueryRepository<Position> query,
                           ICommandRepository<Position> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(PositionCreateDto dto, CancellationToken ct = default)
    {
        var orgId = _userContext.OrganizationId!.Value;

        var exists = await _query.AnyAsync(p => p.Code == dto.Code, ct);
        if (exists)
            return Result.Failure<int>(PositionErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Position
        {
            OrganizationId = orgId,
            Code = dto.Code, 
            Name = dto.Name, 
            StateId = StateIdConst.ACTIVE, 
            CreatedDate = DateTime.Now 
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Position>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(PositionErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<PositionListDto>>> GetAllAsync(PositionListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Position, PositionListDto, PositionListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<PositionDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Position>().Where(x => x.Id == id).As<PositionDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<PositionDto>(PositionErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, PositionUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Position>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(PositionErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code)
        {
            var exists = await _query.AnyAsync(p => p.Code == dto.Code, ct);
            if (exists)
                return Result.Failure(PositionErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
