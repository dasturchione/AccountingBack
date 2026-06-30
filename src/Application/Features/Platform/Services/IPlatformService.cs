using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Platform;

public interface IPlatformService
{
    Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformTenantDto>>> GetTenantsAsync(PlatformTenantListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformTenantDetailDto>> GetTenantByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateTenantAsync(int id, PlatformTenantUpdateDto dto, CancellationToken ct = default);
    Task<Result> ActivateTenantAsync(int id, CancellationToken ct = default);
    Task<Result> DeactivateTenantAsync(int id, CancellationToken ct = default);

    Task<Result<PagedResponse<PlatformUserDto>>> GetUsersAsync(PlatformUserListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformUserDetailDto>> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateUserAsync(PlatformUserCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateUserAsync(int id, PlatformUserUpdateDto dto, CancellationToken ct = default);
    Task<Result> BlockUserAsync(int id, CancellationToken ct = default);
    Task<Result> UnblockUserAsync(int id, CancellationToken ct = default);

    Task<Result<PagedResponse<PlatformOrganizationDto>>> GetOrganizationsAsync(PlatformOrganizationListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformOrganizationDetailDto>> GetOrganizationByIdAsync(int id, CancellationToken ct = default);
    Task<Result> UpdateOrganizationAsync(int id, PlatformOrganizationUpdateDto dto, CancellationToken ct = default);
    Task<Result> ActivateOrganizationAsync(int id, CancellationToken ct = default);
    Task<Result> DeactivateOrganizationAsync(int id, CancellationToken ct = default);
    Task<Result> ArchiveOrganizationAsync(int id, CancellationToken ct = default);

    Task<Result<AccountantWorkspaceDto>> CreateAccountantWorkspaceAsync(AccountantWorkspaceCreateDto dto, CancellationToken ct = default);
    Task<Result<AccountantWorkspaceDto>> GetAccountantWorkspaceAsync(int organizationId, CancellationToken ct = default);

    Task<Result<PlatformUserOrganizationDto>> AttachUserToOrganizationAsync(int userId, PlatformUserOrganizationCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateUserOrganizationAsync(int userId, int organizationId, PlatformUserOrganizationUpdateDto dto, CancellationToken ct = default);
    Task<Result> RemoveUserFromOrganizationAsync(int userId, int organizationId, CancellationToken ct = default);
    Task<Result> SetUserPasswordAsync(int userId, PlatformSetPasswordDto dto, CancellationToken ct = default);

    Task<Result<PagedResponse<PlatformAuditLogDto>>> GetAuditLogsAsync(PlatformAuditLogListFilter filter, CancellationToken ct = default);
}
