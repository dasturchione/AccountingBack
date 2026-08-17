using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Roles;

public class RoleService : IRoleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryBuilder _queryBuilder;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly ICommandRepository<Role> _roleCommand;
    private readonly IQueryRepository<RoleModule> _roleModuleQuery;
    private readonly ICommandRepository<RoleModule> _roleModuleCommand;

    public RoleService(
        IUserContext userContext,
        IQueryBuilder queryBuilder,
        IQueryRepository<Role> roleQuery,
        ICommandRepository<Role> roleCommand,
        IQueryRepository<RoleModule> roleModuleQuery,
        ICommandRepository<RoleModule> roleModuleCommand)
    {
        _userContext = userContext;
        _queryBuilder = queryBuilder;
        _roleQuery = roleQuery;
        _roleCommand = roleCommand;
        _roleModuleQuery = roleModuleQuery;
        _roleModuleCommand = roleModuleCommand;
    }

    public async Task<Result<PagedResponse<RoleListDto>>> GetAllAsync(RoleListFilter filter, CancellationToken ct = default)
    {
        var query = _queryBuilder.BuildPaged<Role, RoleListDto, RoleListFilter>(filter);
        var pagedList = await _roleQuery.GetPagedAsync(query, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var roleSpec = _queryBuilder.For<Role>().Where(role => role.Id == id).As<RoleDto>().Build();
        var dto = await _roleQuery.GetAsync(roleSpec, ct);
        if (dto is null)
            return Result.Failure<RoleDto>(RoleErrors.NotFound(id, _userContext.LanguageId));

        var moduleSpec = _queryBuilder.For<RoleModule>()
            .Where(roleModule => roleModule.RoleId == id)
            .As(roleModule => new RoleModuleDto
            {
                ModuleId = roleModule.ModuleId,
                ModuleCode = roleModule.Module.Code,
                ModuleShortName = roleModule.Module.ShortName,
                ModuleFullName = roleModule.Module.FullName
            })
            .OrderBy(query => query.OrderBy(roleModule => roleModule.ModuleId))
            .Build();
        dto.Modules = await _roleModuleQuery.GetAllAsync(moduleSpec, ct);
        return dto;
    }

    public async Task<Result<int>> CreateAsync(RoleCreateDto dto, CancellationToken ct = default)
    {
        var exists = await _roleQuery.AnyAsync(role => role.ShortName == dto.ShortName, ct);
        if (exists)
            return Result.Failure<int>(RoleErrors.Conflict(dto.ShortName, _userContext.LanguageId));

        var role = new Role
        {
            ShortName = dto.ShortName,
            FullName = dto.FullName,
            Code = dto.Code,
            Description = dto.Description,
            IsSystem = dto.IsSystem,
            SortOrder = dto.SortOrder,
            StateId = StateIdConst.ACTIVE,
            CreatedDate = DateTime.Now
        };
        await _roleCommand.CreateAsync(role, ct);

        if (dto.Modules.Count > 0)
        {
            var roleModules = dto.Modules
                .Distinct()
                .Select(moduleId => new RoleModule
                {
                    RoleId = role.Id,
                    ModuleId = moduleId,
                    CreatedDate = DateTime.Now
                });
            await _roleModuleCommand.CreateAsync(roleModules, ct);
        }

        return role.Id;
    }

    public async Task<Result> UpdateAsync(int id, RoleUpdateDto dto, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Role>().Where(role => role.Id == id).Build();
        var role = await _roleQuery.GetAsync(query, ct);
        if (role is null)
            return Result.Failure(RoleErrors.NotFound(id, _userContext.LanguageId));

        if (role.ShortName != dto.ShortName)
        {
            var exists = await _roleQuery.AnyAsync(item => item.ShortName == dto.ShortName, ct);
            if (exists)
                return Result.Failure(RoleErrors.Conflict(dto.ShortName, _userContext.LanguageId));
        }

        role.ShortName = dto.ShortName;
        role.FullName = dto.FullName;
        role.Code = dto.Code;
        role.Description = dto.Description;
        role.IsSystem = dto.IsSystem;
        role.SortOrder = dto.SortOrder;
        role.StateId = dto.StateId;
        await _roleCommand.UpdateAsync(role, ct);

        await _roleModuleCommand.DeleteAsync(roleModule => roleModule.RoleId == id, ct);
        if (dto.Modules.Count > 0)
        {
            var roleModules = dto.Modules
                .Distinct()
                .Select(moduleId => new RoleModule
                {
                    RoleId = id,
                    ModuleId = moduleId,
                    CreatedDate = DateTime.Now
                });
            await _roleModuleCommand.CreateAsync(roleModules, ct);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var query = _queryBuilder.For<Role>().Where(role => role.Id == id).Build();
        var entity = await _roleQuery.GetAsync(query, ct);
        if (entity is null)
            return Result.Failure(RoleErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _roleCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }
}
