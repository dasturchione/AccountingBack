using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Departments;

public class DepartmentService : BaseService, IDepartmentService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Department> _query;
    private readonly IQueryRepository<Branch> _branchQuery;
    private readonly ICommandRepository<Department> _command;

    public DepartmentService(IUserContext userContext,
                             IQueryBuilder queryBuilder,
                             IQueryRepository<Department> query,
                             IQueryRepository<Branch> branchQuery,
                             ICommandRepository<Department> command,
                             ILogger<DepartmentService> logger, 
                             IUnitOfWork unitOfWork) 
            : base(logger, unitOfWork)
    {
        _query = query;
        _branchQuery = branchQuery;
        _command = command;
        _userContext  = userContext;
        _queryBuilder = queryBuilder;
    }

    public Task<Result<int>> CreateAsync(DepartmentCreateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(CreateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<int>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            if (dto.BranchId.HasValue && !await BranchExistsAsync(dto.BranchId.Value, organizationId, ct))
                return Result.Failure<int>(DepartmentErrors.BranchNotFound(dto.BranchId.Value, _userContext.LanguageId));

            var exists = await _query.AnyAsync(
                d => d.OrganizationId == organizationId && d.Code == dto.Code,
                ct);
            if (exists)
                return Result.Failure<int>(DepartmentErrors.CodeConflict(dto.Code, _userContext.LanguageId));

            var entity = new Department
            {
                OrganizationId = organizationId,
                BranchId       = dto.BranchId,
                Code           = dto.Code,
                Name           = dto.Name,
                StateId        = StateIdConst.ACTIVE,
                CreatedDate    = DateTime.Now
            };

            await _command.CreateAsync(entity, ct);
            return entity.Id;
        });

    public Task<Result> DeleteAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(DeleteAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Department>()
                .Where(d => d.Id == id && d.OrganizationId == organizationId)
                .Build();
            var entity = await _query.GetAsync(query, ct);

            if (entity == null)
                return Result.Failure(DepartmentErrors.NotFound(id, _userContext.LanguageId));

            entity.StateId = StateIdConst.PASSIVE;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    public Task<Result<PagedResponse<DepartmentListDto>>> GetAllAsync(DepartmentListFilter filter, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetAllAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<PagedResponse<DepartmentListDto>>(
                    CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            filter.OrganizationId = organizationId;
            var query = _queryBuilder.BuildPaged<Department, DepartmentListDto, DepartmentListFilter>(filter);
            var pagedList = await _query.GetPagedAsync(query, ct);
            return Result.Success(PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize));
        });

    public Task<Result<DepartmentDto>> GetByIdAsync(int id, CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetByIdAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure<DepartmentDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Department>()
                .Where(d => d.Id == id && d.OrganizationId == organizationId)
                .As<DepartmentDto>()
                .Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure<DepartmentDto>(DepartmentErrors.NotFound(id, _userContext.LanguageId));
            return entity;
        });

    public Task<Result> UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken ct = default) =>
        ExecuteAsync(nameof(UpdateAsync), async () =>
        {
            if (_userContext.OrganizationId is not int organizationId || organizationId <= 0)
                return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

            var query = _queryBuilder.For<Department>()
                .Where(d => d.Id == id && d.OrganizationId == organizationId)
                .Build();
            var entity = await _query.GetAsync(query, ct);
            if (entity == null)
                return Result.Failure(DepartmentErrors.NotFound(id, _userContext.LanguageId));

            if (dto.BranchId.HasValue && !await BranchExistsAsync(dto.BranchId.Value, organizationId, ct))
                return Result.Failure(DepartmentErrors.BranchNotFound(dto.BranchId.Value, _userContext.LanguageId));

            if (entity.Code != dto.Code)
            {
                var exists = await _query.AnyAsync(
                    d => d.OrganizationId == organizationId && d.Code == dto.Code,
                    ct);
                if (exists)
                    return Result.Failure(DepartmentErrors.CodeConflict(dto.Code, _userContext.LanguageId));
            }
            entity.BranchId       = dto.BranchId;
            entity.Code           = dto.Code;
            entity.Name           = dto.Name;
            entity.StateId        = dto.StateId;

            await _command.UpdateAsync(entity, ct);
            return Result.Success();
        });

    private Task<bool> BranchExistsAsync(int branchId, int organizationId, CancellationToken ct) =>
        _branchQuery.AnyAsync(
            branch => branch.Id == branchId && branch.OrganizationId == organizationId,
            ct);
}
