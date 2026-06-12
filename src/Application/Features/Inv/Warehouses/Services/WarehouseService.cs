using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public class WarehouseService : IWarehouseService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Warehouse> _query;
    private readonly ICommandRepository<Warehouse> _command;
    private readonly IQueryBuilder _queryBuilder;

    public WarehouseService(IUserContext userContext,
                            IQueryBuilder queryBuilder, 
                            IQueryRepository<Warehouse> query,
                            ICommandRepository<Warehouse> command)
    {
        _query = query;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(WarehouseCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Warehouse
        {
            OrganizationId = dto.OrganizationId,
            BranchId = dto.BranchId,
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
        var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<WarehouseListDto>>> GetAllAsync(WarehouseListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Warehouse, WarehouseListDto, WarehouseListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == id).As<WarehouseDto>().Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<WarehouseDto>(WarehouseErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, WarehouseUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Warehouse>().Where(x => x.Id == id).Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));

        entity.OrganizationId = dto.OrganizationId;
        entity.BranchId = dto.BranchId;
        entity.Name = dto.Name;
        entity.ResponsibleUserId = dto.ResponsibleUserId;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
