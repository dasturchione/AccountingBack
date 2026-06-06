using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Extensions;
using SharedKernel.Results;

namespace Application.Features.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Department> _query;
    private readonly ICommandRepository<Department> _command;
    private readonly IQueryBuilder<Department> _queryBuilder;

    public DepartmentService(IUserContext userContext, IQueryRepository<Department> query,
        ICommandRepository<Department> command, IQueryBuilder<Department> queryBuilder)
    {
        _userContext  = userContext;
        _query        = query;
        _command      = command;
        _queryBuilder = queryBuilder;
    }

    public async Task<Result<int>> CreateAsync(DepartmentCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _query.AnyAsync(d => d.OrganizationId == dto.OrganizationId && d.Code == dto.Code, ct);
        if (exists)
            return Result.Failure<int>(DepartmentErrors.CodeConflict(dto.Code, _userContext.LanguageId));

        var entity = new Department
        {
            OrganizationId = dto.OrganizationId,
            BranchId       = dto.BranchId,
            Code           = dto.Code,
            Name           = dto.Name,
            StateId        = StateIdConst.ACTIVE,
            CreatedDate    = DateTime.Now
        };

        await _command.CreateAsync(entity, ct);
        return entity.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(DepartmentErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<DepartmentListDto>>> GetAllAsync(DepartmentListFilter filter, CancellationToken ct = default)
    {
        var spec      = _queryBuilder.BuildPaged<DepartmentListDto, DepartmentListFilter>(filter);
        var pagedList = await _query.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById<Department, DepartmentDto>(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<DepartmentDto>(DepartmentErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken ct = default)
    {
        var spec   = _queryBuilder.ById(id);
        var entity = await _query.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(DepartmentErrors.NotFound(id, _userContext.LanguageId));

        if (entity.Code != dto.Code)
        {
            var exists = await _query.AnyAsync(d => d.OrganizationId == dto.OrganizationId && d.Code == dto.Code, ct);
            if (exists)
                return Result.Failure(DepartmentErrors.CodeConflict(dto.Code, _userContext.LanguageId));
        }

        entity.OrganizationId = dto.OrganizationId;
        entity.BranchId       = dto.BranchId;
        entity.Code           = dto.Code;
        entity.Name           = dto.Name;
        entity.StateId        = dto.StateId;

        await _command.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
