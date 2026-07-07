using Application.Features.Platform;
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

    [HttpGet("users")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetUsers)]
    public async Task<IResult> GetUsersAsync([FromQuery] PlatformUserListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetUsersAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("users/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetUserById)]
    public async Task<IResult> GetUserByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.GetUserByIdAsync(id, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("users")]
    [ModuleAuthorize(PermissionCodeConst.PlatformCreateUser)]
    public async Task<IResult> CreateUserAsync([FromBody] PlatformUserCreateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.CreateUserAsync(dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("users/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateUser)]
    public async Task<IResult> UpdateUserAsync([FromRoute] int id, [FromBody] PlatformUserUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.UpdateUserAsync(id, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("users/{id:int}/block")]
    [ModuleAuthorize(PermissionCodeConst.PlatformBlockUser)]
    public async Task<IResult> BlockUserAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.BlockUserAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("users/{id:int}/unblock")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUnblockUser)]
    public async Task<IResult> UnblockUserAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.UnblockUserAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpGet("organizations")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetOrganizations)]
    public async Task<IResult> GetOrganizationsAsync([FromQuery] PlatformOrganizationListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetOrganizationsAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("organizations/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetOrganizationById)]
    public async Task<IResult> GetOrganizationByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.GetOrganizationByIdAsync(id, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("organizations/{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateOrganization)]
    public async Task<IResult> UpdateOrganizationAsync([FromRoute] int id, [FromBody] PlatformOrganizationUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.UpdateOrganizationAsync(id, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("organizations/{id:int}/activate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformActivateOrganization)]
    public async Task<IResult> ActivateOrganizationAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.ActivateOrganizationAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("organizations/{id:int}/deactivate")]
    [ModuleAuthorize(PermissionCodeConst.PlatformDeactivateOrganization)]
    public async Task<IResult> DeactivateOrganizationAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.DeactivateOrganizationAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("organizations/{id:int}/archive")]
    [ModuleAuthorize(PermissionCodeConst.PlatformArchiveOrganization)]
    public async Task<IResult> ArchiveOrganizationAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _platformService.ArchiveOrganizationAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("accountant-workspaces")]
    [ModuleAuthorize(PermissionCodeConst.PlatformCreateAccountantWorkspace)]
    public async Task<IResult> CreateAccountantWorkspaceAsync([FromBody] AccountantWorkspaceCreateDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.CreateAccountantWorkspaceAsync(dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("accountant-workspaces/{organizationId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetAccountantWorkspace)]
    public async Task<IResult> GetAccountantWorkspaceAsync([FromRoute] int organizationId, CancellationToken ct = default)
    {
        var response = await _platformService.GetAccountantWorkspaceAsync(organizationId, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost("users/{userId:int}/organizations")]
    [ModuleAuthorize(PermissionCodeConst.PlatformAttachUserToOrganization)]
    public async Task<IResult> AttachUserToOrganizationAsync(
        [FromRoute] int userId,
        [FromBody] PlatformUserOrganizationCreateDto dto,
        CancellationToken ct = default)
    {
        var response = await _platformService.AttachUserToOrganizationAsync(userId, dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("users/{userId:int}/organizations/{organizationId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformUpdateUserOrganization)]
    public async Task<IResult> UpdateUserOrganizationAsync(
        [FromRoute] int userId,
        [FromRoute] int organizationId,
        [FromBody] PlatformUserOrganizationUpdateDto dto,
        CancellationToken ct = default)
    {
        var response = await _platformService.UpdateUserOrganizationAsync(userId, organizationId, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("users/{userId:int}/organizations/{organizationId:int}")]
    [ModuleAuthorize(PermissionCodeConst.PlatformRemoveUserFromOrganization)]
    public async Task<IResult> RemoveUserFromOrganizationAsync(
        [FromRoute] int userId,
        [FromRoute] int organizationId,
        CancellationToken ct = default)
    {
        var response = await _platformService.RemoveUserFromOrganizationAsync(userId, organizationId, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpPost("users/{userId:int}/set-password")]
    [ModuleAuthorize(PermissionCodeConst.PlatformSetUserPassword)]
    public async Task<IResult> SetUserPasswordAsync([FromRoute] int userId, [FromBody] PlatformSetPasswordDto dto, CancellationToken ct = default)
    {
        var response = await _platformService.SetUserPasswordAsync(userId, dto, ct);
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
