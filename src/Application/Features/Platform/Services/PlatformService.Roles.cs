using Application.Abstractions.Authentication;
using Application.Common.Pagination;
using Application.Features.Roles;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.QueryResults;
using SharedKernel.Results;

namespace Application.Features.Platform;

public sealed partial class PlatformService
{
    public Task<Result<PagedResponse<RoleListDto>>> GetOrganizationRolesAsync(
        int tenantId,
        int organizationId,
        RoleListFilter filter,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetOrganizationRolesAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<PagedResponse<RoleListDto>>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<PagedResponse<RoleListDto>>(PlatformErrors.TenantNotFound(tenantId));

            if (await GetTenantOrganizationAsync(tenantId, organizationId, ct) is null)
                return Result.Failure<PagedResponse<RoleListDto>>(PlatformErrors.OrganizationNotFound(organizationId));

            var page = Math.Max(filter.Page, 1);
            var pageSize = filter.PageSize is > 0 ? filter.PageSize.Value : 50;
            var search = filter.Search?.Trim().ToLowerInvariant();
            var spec = _queryBuilder.For<Role>()
                .Where(role =>
                    role.OrganizationId == organizationId &&
                    (string.IsNullOrWhiteSpace(search) ||
                     role.ShortName.ToLower().Contains(search) ||
                     role.FullName.ToLower().Contains(search)))
                .OrderBy(query => query.OrderBy(role => role.SortOrder).ThenBy(role => role.Id))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AddIncludes(builder => builder.Include(role => role.State))
                .BuildPaged();

            var paged = await _roleQuery.GetPagedAsync(spec, ct);
            var items = paged.Items.Select(role => new RoleListDto
            {
                Id = role.Id,
                ShortName = role.ShortName,
                FullName = role.FullName,
                Code = role.Code,
                Description = role.Description,
                IsSystem = role.IsSystem,
                SortOrder = role.SortOrder,
                StateId = role.StateId,
                StateName = role.State.FullName,
                CreatedDate = role.CreatedDate
            }).ToList();

            return PagedResponseFactory.Create(
                new PagedList<RoleListDto>(items, paged.TotalCount),
                page,
                pageSize);
        });

    public Task<Result<RoleDto>> GetOrganizationRoleByIdAsync(
        int tenantId,
        int organizationId,
        int roleId,
        CancellationToken ct = default) =>
        ExecuteAsync(nameof(GetOrganizationRoleByIdAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<RoleDto>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<RoleDto>(PlatformErrors.TenantNotFound(tenantId));

            if (await GetTenantOrganizationAsync(tenantId, organizationId, ct) is null)
                return Result.Failure<RoleDto>(PlatformErrors.OrganizationNotFound(organizationId));

            var query = _queryBuilder.For<Role>()
                .Where(role => role.Id == roleId && role.OrganizationId == organizationId)
                .Build();
            query.AddIncludes(builder => builder.Include(role => role.State));
            var role = await _roleQuery.GetAsync(query, ct);
            if (role is null)
                return Result.Failure<RoleDto>(RoleErrors.NotFound(roleId, _userContext.LanguageId));

            var dto = new RoleDto
            {
                Id = role.Id,
                ShortName = role.ShortName,
                FullName = role.FullName,
                Code = role.Code,
                Description = role.Description,
                IsSystem = role.IsSystem,
                SortOrder = role.SortOrder,
                StateId = role.StateId,
                StateName = role.State.FullName,
                CreatedDate = role.CreatedDate
            };
            var moduleSpec = _queryBuilder.For<RoleModule>()
                .Where(roleModule => roleModule.RoleId == roleId)
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
        });

    public Task<Result<int>> CreateOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        RoleCreateDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(CreateOrganizationRoleAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure<int>(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure<int>(PlatformErrors.TenantNotFound(tenantId));

            if (await GetTenantOrganizationAsync(tenantId, organizationId, ct) is null)
                return Result.Failure<int>(PlatformErrors.OrganizationNotFound(organizationId));

            var shortName = dto.ShortName.Trim();
            if (await _roleQuery.AnyAsync(
                    role => role.OrganizationId == organizationId && role.ShortName == shortName,
                    ct))
            {
                return Result.Failure<int>(RoleErrors.Conflict(shortName, _userContext.LanguageId));
            }

            var now = DateTime.Now;
            var role = new Role
            {
                ShortName = shortName,
                FullName = dto.FullName.Trim(),
                Code = dto.Code,
                Description = dto.Description,
                IsSystem = false,
                SortOrder = dto.SortOrder,
                StateId = StateIdConst.ACTIVE,
                OrganizationId = organizationId,
                CreatedDate = now
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
                        CreatedDate = now
                    });
                await _roleModuleCommand.CreateAsync(roleModules, ct);
            }

            return role.Id;
        }, ct);

    public Task<Result> UpdateOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        int roleId,
        RoleUpdateDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(UpdateOrganizationRoleAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure(PlatformErrors.TenantNotFound(tenantId));

            if (await GetTenantOrganizationAsync(tenantId, organizationId, ct) is null)
                return Result.Failure(PlatformErrors.OrganizationNotFound(organizationId));

            var query = _queryBuilder.For<Role>()
                .Where(role => role.Id == roleId && role.OrganizationId == organizationId)
                .Build();
            var role = await _roleQuery.GetAsync(query, ct);
            if (role is null)
                return Result.Failure(RoleErrors.NotFound(roleId, _userContext.LanguageId));

            var shortName = dto.ShortName.Trim();
            if (role.ShortName != shortName && await _roleQuery.AnyAsync(
                    item => item.OrganizationId == organizationId &&
                            item.ShortName == shortName &&
                            item.Id != roleId,
                    ct))
            {
                return Result.Failure(RoleErrors.Conflict(shortName, _userContext.LanguageId));
            }

            role.ShortName = shortName;
            role.FullName = dto.FullName.Trim();
            role.Code = dto.Code;
            role.Description = dto.Description;
            role.IsSystem = false;
            role.SortOrder = dto.SortOrder;
            role.StateId = dto.StateId;
            await _roleCommand.UpdateAsync(role, ct);

            await _roleModuleCommand.DeleteAsync(roleModule => roleModule.RoleId == roleId, ct);
            if (dto.Modules.Count > 0)
            {
                var roleModules = dto.Modules
                    .Distinct()
                    .Select(moduleId => new RoleModule
                    {
                        RoleId = roleId,
                        ModuleId = moduleId,
                        CreatedDate = DateTime.Now
                    });
                await _roleModuleCommand.CreateAsync(roleModules, ct);
            }

            return Result.Success();
        }, ct);

    public Task<Result> DeleteOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        int roleId,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync(nameof(DeleteOrganizationRoleAsync), async () =>
        {
            if (_userContext.UserKind != CurrentUserKind.SuperAdmin)
                return Result.Failure(PlatformErrors.GlobalAccessRequired());

            if (await GetTenantEntityAsync(tenantId, ct) is null)
                return Result.Failure(PlatformErrors.TenantNotFound(tenantId));

            if (await GetTenantOrganizationAsync(tenantId, organizationId, ct) is null)
                return Result.Failure(PlatformErrors.OrganizationNotFound(organizationId));

            var query = _queryBuilder.For<Role>()
                .Where(role => role.Id == roleId && role.OrganizationId == organizationId)
                .Build();
            var role = await _roleQuery.GetAsync(query, ct);
            if (role is null)
                return Result.Failure(RoleErrors.NotFound(roleId, _userContext.LanguageId));

            role.StateId = StateIdConst.PASSIVE;
            await _roleCommand.UpdateAsync(role, ct);
            return Result.Success();
        }, ct);
}
