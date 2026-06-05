using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Common.Factory;
using Application.Common.Pagination;
using Application.Options;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Results;

namespace Application.Features.Roles;

public class RoleService : IRoleService
{
    private readonly IUserContext _userContext;
    private readonly IQueryRepository<Role> _roleQuery;
    private readonly ICommandRepository<Role> _roleCommand;
    private readonly ISpecificationFactory<Role> _roleSpecification;

    public RoleService(
        IUserContext userContext,
        IQueryRepository<Role> roleQuery,
        ICommandRepository<Role> roleCommand,
        ISpecificationFactory<Role> roleSpecification)
    {
        _userContext      = userContext;
        _roleQuery        = roleQuery;
        _roleCommand      = roleCommand;
        _roleSpecification = roleSpecification;
    }

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
        return role.Id;
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var spec   = _roleSpecification.Build(new GetByIdOptions<int>(id));
        var entity = await _roleQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure(RoleErrors.NotFound(id, _userContext.LanguageId));

        entity.StateId = StateIdConst.PASSIVE;
        await _roleCommand.UpdateAsync(entity, ct);
        return Result.Success();
    }

    public async Task<Result<PagedResponse<RoleListDto>>> GetAllAsync(RoleListFilter filter, CancellationToken ct = default)
    {
        var spec      = _roleSpecification.BuildPaged<RoleListDto, RoleListFilter>(filter);
        var pagedList = await _roleQuery.GetPagedAsync(spec, ct);
        return PagedResponseFactory.Create(pagedList, filter.Page, filter.PageSize);
    }

    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var spec   = _roleSpecification.Build<RoleDto, GetByIdOptions<int>>(new GetByIdOptions<int>(id));
        var entity = await _roleQuery.GetAsync(spec, ct);
        if (entity == null)
            return Result.Failure<RoleDto>(RoleErrors.NotFound(id, _userContext.LanguageId));
        return entity;
    }

    public async Task<Result> UpdateAsync(int id, RoleUpdateDto dto, CancellationToken ct = default)
    {
        var spec = _roleSpecification.Build(new GetByIdOptions<int>(id));
        var role = await _roleQuery.GetAsync(spec, ct);
        if (role == null)
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
        return Result.Success();
    }
}
