using Application.Common.Pagination;
using Application.Features.Platform.Filters;
using SharedKernel.Results;

namespace Application.Features.Platform;

public interface IPlatformService
{
    Task<Result<PlatformDashboardDto>> GetDashboardAsync(CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformTenantDto>>> GetTenantsAsync(PlatformTenantListFilter filter, CancellationToken ct = default);
    Task<Result<PlatformTenantDto>> GetTenantByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateTenantAsync(PlatformTenantCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateTenantAsync(int id, PlatformTenantUpdateDto dto, CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformUserDto>>> GetTenantUsersAsync(int tenantId, PlatformUserListFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformOrganizationDto>>> GetTenantOrganizationsAsync(int tenantId, PlatformOrganizationListFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<PlatformAuditLogDto>>> GetAuditLogsAsync(PlatformAuditLogListFilter filter, CancellationToken ct = default);
}
