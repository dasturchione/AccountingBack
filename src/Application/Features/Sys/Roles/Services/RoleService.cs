using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Query.Specifications;
using SharedKernel.Results;

namespace Application.Features.Roles;

public class RoleService : IRoleService
{
    private readonly IUserContext                    _userContext;
    private readonly IQueryBuilder                   _queryBuilder;
    private readonly IQueryRepository<Role>          _roleQuery;
    private readonly ICommandRepository<Role>        _roleCommand;
    private readonly IQueryRepository<RoleModule>    _roleModuleQuery;
    private readonly ICommandRepository<RoleModule>  _roleModuleCommand;

    public RoleService(
        IUserContext                   userContext,
        IQueryBuilder                  queryBuilder,
        IQueryRepository<Role>         roleQuery,
        ICommandRepository<Role>       roleCommand,
        IQueryRepository<RoleModule>   roleModuleQuery,
        ICommandRepository<RoleModule> roleModuleCommand)
    {
        _userContext       = userContext;
        _queryBuilder      = queryBuilder;
        _roleQuery         = roleQuery;
        _roleCommand       = roleCommand;
        _roleModuleQuery   = roleModuleQuery;
        _roleModuleCommand = roleModuleCommand;
    }

    // ------------------------------------------------------------------ //
    //  GET ALL
    // ------------------------------------------------------------------ //
    public async Task<Result<PagedResponse<RoleListDto>>> GetAllAsync(RoleListFilter filter, CancellationToken ct = default)
    {
        var query     = _queryBuilder.BuildPaged<Role, RoleListDto, RoleListFilter>(filter);
        var pagedList = await _roleQuery.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    // ------------------------------------------------------------------ //
    //  GET BY ID  (role + its modules)
    // ------------------------------------------------------------------ //
    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // 1. Load role → RoleDto via projection (Id, ShortName, FullName, State…)
        var roleSpec = _queryBuilder.For<Role>().Where(r => r.Id == id).As<RoleDto>().Build();
        var dto      = await _roleQuery.GetAsync(roleSpec, ct);
        if (dto is null)
            return Result.Failure<RoleDto>(RoleErrors.NotFound(id, _userContext.LanguageId));

        // 2. Load assigned modules for this role
        var moduleSpec = new QuerySpecification<RoleModule, RoleModuleDto>
        {
            Criteria = rm => rm.RoleId == id,
            OrderBy  = q  => q.OrderBy(rm => rm.ModuleId),
            Selector = rm => new RoleModuleDto
            {
                ModuleId        = rm.ModuleId,
                ModuleCode      = rm.Module.Code,
                ModuleShortName = rm.Module.ShortName,
                ModuleFullName  = rm.Module.FullName
            }
        };
        dto.Modules = await _roleModuleQuery.GetAllAsync(moduleSpec, ct);

        return dto;
    }

    // ------------------------------------------------------------------ //
    //  CREATE
    // ------------------------------------------------------------------ //
    public async Task<Result<int>> CreateAsync(RoleCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _roleQuery.AnyAsync(r => r.ShortName == dto.ShortName, ct);
        if (exists)
            return Result.Failure<int>(RoleErrors.Conflict(dto.ShortName, _userContext.LanguageId));

        var role = new Role
        {
            ShortName   = dto.ShortName,
            FullName    = dto.FullName,
            StateId     = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };

        await _roleCommand.CreateAsync(role, ct);

        // Assign modules
        if (dto.Modules.Count > 0)
        {
            var roleModules = dto.Modules
                .Distinct()
                .Select(moduleId => new RoleModule
                {
                    RoleId      = role.Id,
                    ModuleId    = moduleId,
                    CreatedDate = DateTime.Now
                });
            await _roleModuleCommand.CreateAsync(roleModules, ct);
        }

        return role.Id;
    }

    // ------------------------------------------------------------------ //
    //  UPDATE
    // ------------------------------------------------------------------ //
    public async Task<Result> UpdateAsync(int id, RoleUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Role>().Where(r => r.Id == id).Build();
        var role  = await _roleQuery.GetAsync(query, ct);
        if (role is null)
            return Result.Failure(RoleErrors.NotFound(id, _userContext.LanguageId));

        if (role.ShortName != dto.ShortName)
        {
            var exists = await _roleQuery.AnyAsync(r => r.ShortName == dto.ShortName, ct);
            if (exists)
                return Result.Failure(RoleErrors.Conflict(dto.ShortName, _userContext.LanguageId));
        }

        role.ShortName = dto.ShortName;
        role.FullName  = dto.FullName;
        role.StateId   = dto.StateId;

        await _roleCommand.UpdateAsync(role, ct);

        // Sync modules: delete old → insert new
        await _roleModuleCommand.DeleteAsync(rm => rm.RoleId == id, ct);

        if (dto.Modules.Count > 0)
        {
            var roleModules = dto.Modules
                .Distinct()
                .Select(moduleId => new RoleModule
                {
                    RoleId      = id,
                    ModuleId    = moduleId,
                    CreatedDate = DateTime.Now
                });
            await _roleModuleCommand.CreateAsync(roleModules, ct);
        }

        return Result.Success();
    }

    // ------------------------------------------------------------------ //
    //  DELETE  (soft delete)
    // ------------------------------------------------------------------ //
    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query  = _queryBuilder.For<Role>().Where(r => r.Id == id).Build();
        var entity = await _roleQuery.GetAsync(query, ct);
        if (entity is null)
            return Result.Failure(RoleErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _roleCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
