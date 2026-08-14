using Application.Common.Pagination;
using Application.Features.Organizations;
using Application.Features.Platform.Filters;
using Application.Features.Roles;
using SharedKernel.Results;

namespace Application.Features.Platform;

public interface IPlatformService
{
    Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformTenantDto>>> GetTenantsAsync(PlatformTenantListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformTenantDto>> GetTenantByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateTenantAsync(int id, PlatformTenantUpdateDto dto, CancellationToken ct = default);
    Task<Result> ActivateTenantAsync(int id, CancellationToken ct = default);
    Task<Result> DeactivateTenantAsync(int id, CancellationToken ct = default);

    Task<Result<PagedResponse<PlatformUserDto>>> GetTenantUsersAsync(int tenantId, PlatformUserListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformUserDetailDto>> GetTenantUserByIdAsync(int tenantId, int userId, CancellationToken ct = default);
    Task<Result<int>> CreateTenantUserAsync(int tenantId, PlatformUserCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateTenantUserAsync(int tenantId, int userId, PlatformUserUpdateDto dto, CancellationToken ct = default);
    Task<Result> BlockTenantUserAsync(int tenantId, int userId, CancellationToken ct = default);
    Task<Result> UnblockTenantUserAsync(int tenantId, int userId, CancellationToken ct = default);
    Task<Result> SetTenantUserPasswordAsync(int tenantId, int userId, PlatformSetPasswordDto dto, CancellationToken ct = default);

    Task<Result<PagedResponse<PlatformOrganizationDto>>> GetTenantOrganizationsAsync(int tenantId, PlatformOrganizationListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformOrganizationDetailDto>> GetTenantOrganizationByIdAsync(int tenantId, int organizationId, CancellationToken ct = default);
    Task<Result<int>> CreateTenantOrganizationAsync(int tenantId, OrganizationCreateDto dto, CancellationToken ct = default);
    Task<Result> ActivateTenantOrganizationAsync(int tenantId, int organizationId, CancellationToken ct = default);
    Task<Result> DeactivateTenantOrganizationAsync(int tenantId, int organizationId, CancellationToken ct = default);

    Task<Result<PagedResponse<RoleListDto>>> GetOrganizationRolesAsync(
        int tenantId,
        int organizationId,
        RoleListFilter filter,
        CancellationToken ct = default);
    Task<Result<RoleDto>> GetOrganizationRoleByIdAsync(
        int tenantId,
        int organizationId,
        int roleId,
        CancellationToken ct = default);
    Task<Result<int>> CreateOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        RoleCreateDto dto,
        CancellationToken ct = default);
    Task<Result> UpdateOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        int roleId,
        RoleUpdateDto dto,
        CancellationToken ct = default);
    Task<Result> DeleteOrganizationRoleAsync(
        int tenantId,
        int organizationId,
        int roleId,
        CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformAuditLogDto>>> GetAuditLogsAsync(PlatformAuditLogListFilter filter, CancellationToken ct = default);
}
