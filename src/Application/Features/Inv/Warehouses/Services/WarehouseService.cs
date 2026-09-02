using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Warehouses;

public class WarehouseService : IWarehouseService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Warehouse> _query;
    private readonly IQueryRepository<Branch> _branchQuery;
    private readonly IQueryRepository<UserOrganization> _userOrganizationQuery;
    private readonly ICommandRepository<Warehouse> _command;
    private readonly IQueryBuilder _queryBuilder;

    public WarehouseService(IUserContext userContext,
                            IQueryBuilder queryBuilder, 
                            IQueryRepository<Warehouse> query,
                            IQueryRepository<Branch> branchQuery,
                            IQueryRepository<UserOrganization> userOrganizationQuery,
                            ICommandRepository<Warehouse> command)
    {
        _query = query;
        _branchQuery = branchQuery;
        _userOrganizationQuery = userOrganizationQuery;
        _command = command;
        _userContext = userContext; 
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(WarehouseCreateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        if (dto.Code is not null &&
            await _query.AnyAsync(
                warehouse => warehouse.OrganizationId == organizationId && warehouse.Code == dto.Code,
                ct))
            return Result.Failure<int>(WarehouseErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var referenceValidation = await ValidateReferencesAsync(
            dto.BranchId,
            dto.ResponsibleUserId,
            organizationId,
            ct);
        if (!referenceValidation.IsSuccess)
            return Result.Failure<int>(referenceValidation.Error);

        var entity = new Warehouse
        {
            OrganizationId = organizationId,
            BranchId = dto.BranchId,
            Code = dto.Code,
            Name = dto.Name,
            Address = dto.Address,
            IsMain = dto.IsMain,
            ResponsibleUserId = dto.ResponsibleUserId,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<Warehouse>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);

        if (entity == null) 
            return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<WarehouseListDto>>> GetAllAsync(WarehouseListFilter filter, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<PagedResponse<WarehouseListDto>>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        filter.OrganizationId = organizationId;
        var query = _queryBuilder.BuildPaged<Warehouse, WarehouseListDto, WarehouseListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<WarehouseDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure<WarehouseDto>(
                CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<Warehouse>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .As<WarehouseDto>()
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure<WarehouseDto>(WarehouseErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, WarehouseUpdateDto dto, CancellationToken ct = default)
    {
        if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
            return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

        var query = _queryBuilder.For<Warehouse>()
            .Where(x => x.Id == id && x.OrganizationId == organizationId)
            .Build();
        var entity = await _query.GetAsync(query, ct);
        if (entity == null) 
            return Result.Failure(WarehouseErrors.NotFound(id, _userContext.LanguageId));

        if (dto.Code is not null &&
            entity.Code != dto.Code &&
            await _query.AnyAsync(
                warehouse => warehouse.OrganizationId == organizationId && warehouse.Code == dto.Code,
                ct))
            return Result.Failure(WarehouseErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var referenceValidation = await ValidateReferencesAsync(
            dto.BranchId,
            dto.ResponsibleUserId,
            organizationId,
            ct);
        if (!referenceValidation.IsSuccess)
            return referenceValidation;

        entity.BranchId = dto.BranchId;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.IsMain = dto.IsMain;
        entity.ResponsibleUserId = dto.ResponsibleUserId;
        entity.StateId = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    private async Task<Result> ValidateReferencesAsync(
        int? branchId,
        int? responsibleUserId,
        int organizationId,
        CancellationToken ct)
    {
        if (branchId.HasValue &&
            !await _branchQuery.AnyAsync(
                branch => branch.Id == branchId.Value &&
                          branch.OrganizationId == organizationId &&
                          branch.StateId == StateIdConst.ACTIVE,
                ct))
            return Result.Failure(WarehouseErrors.BranchNotFound(branchId.Value, _userContext.LanguageId));

        if (responsibleUserId.HasValue &&
            !await _userOrganizationQuery.AnyAsync(
                membership => membership.UserId == responsibleUserId.Value &&
                              membership.OrganizationId == organizationId &&
                              membership.StateId == StateIdConst.ACTIVE,
                ct))
            return Result.Failure(WarehouseErrors.ResponsibleUserNotFound(
                responsibleUserId.Value,
                _userContext.LanguageId));

        return Result.Success();
    }
}
