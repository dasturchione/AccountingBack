using Application.Features.Organizations;
using Application.Features.Platform;
using Application.Features.Platform.Filters;
using Application.Features.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/platform")]
[ApiController]
[Authorize]
[GlobalAccessAuthorize]
public sealed class PlatformController : ControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet("dashboard")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetDashboard)]
    public async Task<IResult> GetDashboardAsync(CancellationToken ct = default)
    {
        var response = await _platformService.GetDashboardAsync(ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetTenants)]
    public async Task<IResult> GetTenantsAsync([FromQuery] PlatformTenantListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantsAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetTenantById)]
    public async Task<IResult> GetTenantByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantByIdAsync(id, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("tenants")]
    [ModuleAuthorize(PermissionCodeConst.PlatformCreateTenant)]
    public async Task<IResult> CreateTenantAsync([FromBody] PlatformTenantCreateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.CreateTenantAsync(dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("tenants/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateTenant)]
    public async Task<IResult> UpdateTenantAsync([FromRoute] int id, [FromBody] PlatformTenantUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.UpdateTenantAsync(id, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{id:int}/activate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformActivateTenant)]
    public async Task<IResult> ActivateTenantAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.ActivateTenantAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{id:int}/deactivate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformDeactivateTenant)]
    public async Task<IResult> DeactivateTenantAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.DeactivateTenantAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("tenants/{id:int}/users")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetUsers)]
    public async Task<IResult> GetTenantUsersAsync([FromRoute] int id, [FromQuery] PlatformUserListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantUsersAsync(id, filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants/{tenantId:int}/users/{userId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetUserById)]
    public async Task<IResult> GetTenantUserByIdAsync([FromRoute] int tenantId, [FromRoute] int userId, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantUserByIdAsync(tenantId, userId, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("tenants/{id:int}/users")]
    [ModuleAuthorize(PermissionCodeConst.PlatformCreateUser)]
    public async Task<IResult> CreateTenantUserAsync([FromRoute] int id, [FromBody] PlatformUserCreateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.CreateTenantUserAsync(id, dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("tenants/{tenantId:int}/users/{userId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateUser)]
    public async Task<IResult> UpdateTenantUserAsync([FromRoute] int tenantId, [FromRoute] int userId, [FromBody] PlatformUserUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.UpdateTenantUserAsync(tenantId, userId, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/users/{userId:int}/block")]
    [ModuleAuthorize(PermissionCodeConst.PlatformBlockUser)]
    public async Task<IResult> BlockTenantUserAsync([FromRoute] int tenantId, [FromRoute] int userId, CancellationToken ct = default)
    {
        var response = await _platformService.BlockTenantUserAsync(tenantId, userId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/users/{userId:int}/unblock")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUnblockUser)]
    public async Task<IResult> UnblockTenantUserAsync([FromRoute] int tenantId, [FromRoute] int userId, CancellationToken ct = default)
    {
        var response = await _platformService.UnblockTenantUserAsync(tenantId, userId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/users/{userId:int}/set-password")]
    [ModuleAuthorize(PermissionCodeConst.PlatformSetUserPassword)]
    public async Task<IResult> SetTenantUserPasswordAsync([FromRoute] int tenantId, [FromRoute] int userId, [FromBody] PlatformSetPasswordDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.SetTenantUserPasswordAsync(tenantId, userId, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("tenants/{id:int}/organizations")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetOrganizations)]
    public async Task<IResult> GetTenantOrganizationsAsync([FromRoute] int id, [FromQuery] PlatformOrganizationListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantOrganizationsAsync(id, filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants/{tenantId:int}/organizations/{organizationId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetOrganizationById)]
    public async Task<IResult> GetTenantOrganizationByIdAsync([FromRoute] int tenantId, [FromRoute] int organizationId, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantOrganizationByIdAsync(tenantId, organizationId, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("tenants/{id:int}/organizations")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateOrganization)]
    public async Task<IResult> CreateTenantOrganizationAsync([FromRoute] int id, [FromBody] OrganizationCreateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.CreateTenantOrganizationAsync(id, dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/organizations/{organizationId:int}/activate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformActivateOrganization)]
    public async Task<IResult> ActivateTenantOrganizationAsync([FromRoute] int tenantId, [FromRoute] int organizationId, CancellationToken ct = default)
    {
        var response = await _platformService.ActivateTenantOrganizationAsync(tenantId, organizationId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/organizations/{organizationId:int}/deactivate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformDeactivateOrganization)]
    public async Task<IResult> DeactivateTenantOrganizationAsync([FromRoute] int tenantId, [FromRoute] int organizationId, CancellationToken ct = default)
    {
        var response = await _platformService.DeactivateTenantOrganizationAsync(tenantId, organizationId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("tenants/{tenantId:int}/organizations/{organizationId:int}/roles")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetRoles)]
    public async Task<IResult> GetOrganizationRolesAsync(
        [FromRoute] int tenantId,
        [FromRoute] int organizationId,
        [FromQuery] RoleListFilter filter,
        CancellationToken ct = default)
    {
        var response = await _platformService.GetOrganizationRolesAsync(tenantId, organizationId, filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants/{tenantId:int}/organizations/{organizationId:int}/roles/{roleId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetRoleById)]
    public async Task<IResult> GetOrganizationRoleByIdAsync(
        [FromRoute] int tenantId,
        [FromRoute] int organizationId,
        [FromRoute] int roleId,
        CancellationToken ct = default)
    {
        var response = await _platformService.GetOrganizationRoleByIdAsync(tenantId, organizationId, roleId, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("tenants/{tenantId:int}/organizations/{organizationId:int}/roles")]
    [ModuleAuthorize(PermissionCodeConst.PlatformCreateRole)]
    public async Task<IResult> CreateOrganizationRoleAsync(
        [FromRoute] int tenantId,
        [FromRoute] int organizationId,
        [FromBody] RoleCreateDto dto,
        CancellationToken ct = default)
    {
        var response = await _platformService.CreateOrganizationRoleAsync(tenantId, organizationId, dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("tenants/{tenantId:int}/organizations/{organizationId:int}/roles/{roleId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateRole)]
    public async Task<IResult> UpdateOrganizationRoleAsync(
        [FromRoute] int tenantId,
        [FromRoute] int organizationId,
        [FromRoute] int roleId,
        [FromBody] RoleUpdateDto dto,
        CancellationToken ct = default)
    {
        var response = await _platformService.UpdateOrganizationRoleAsync(tenantId, organizationId, roleId, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("tenants/{tenantId:int}/organizations/{organizationId:int}/roles/{roleId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformDeleteRole)]
    public async Task<IResult> DeleteOrganizationRoleAsync(
        [FromRoute] int tenantId,
        [FromRoute] int organizationId,
        [FromRoute] int roleId,
        CancellationToken ct = default)
    {
        var response = await _platformService.DeleteOrganizationRoleAsync(tenantId, organizationId, roleId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("audit-logs")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetAuditLogs)]
    public async Task<IResult> GetAuditLogsAsync([FromQuery] PlatformAuditLogListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetAuditLogsAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }
}
