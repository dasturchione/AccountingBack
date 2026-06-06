using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public class WarehouseService : IWarehouseService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Warehouse> _query;
    private readonly ICommandRepository<Warehouse> _command;
    private readonly IQueryBuilder<Warehouse> _queryBuilder;

    public WarehouseService(IUserContext userContext, IQueryRepository<Warehouse> query,
        ICommandRepository<Warehouse> command, IQueryBuilder<Warehouse> queryBuilder)
    {
        _userContext = userContext; _query = query; _command = command; _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(WarehouseCreateDto dto, CancellationToken ct = default)
    {
        if (await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure<int>(WarehouseErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Warehouse
        {
            OrganizationId = dto.OrganizationId,
            BranchId = dto.BranchId,
            Code = dto.Code,
            Name = dto.Name,
            ResponsibleUserId = dto.ResponsibleUserId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));
        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<WarehouseListDto>>> GetAllAsync(WarehouseListFilter filter, CancellationToken ct = default)
    {
        var pagedList = await _query.GetPagedAsync(_queryBuilder.BuildPaged<WarehouseListDto, WarehouseListFilter>(filter), ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById<Warehouse, WarehouseDto>(id), ct);
        if (entity == null) return Result.Failure<WarehouseDto>(WarehouseErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, WarehouseUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _query.GetAsync(_queryBuilder.ById(id), ct);
        if (entity == null) return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code && await _query.AnyAsync(x => x.OrganizationId == dto.OrganizationId && x.Code == dto.Code, ct))
            return Result.Failure(WarehouseErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.BranchId = dto.BranchId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.ResponsibleUserId = dto.ResponsibleUserId;
        entity.StateId = dto.StateId;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
