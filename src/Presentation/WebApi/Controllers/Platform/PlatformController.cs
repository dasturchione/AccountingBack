using Application.Features.Platform;
using Application.Features.Platform.Filters;
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

    [HttpGet("tenants/{id:int}/users")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetUsers)]
    public async Task<IResult> GetTenantUsersAsync([FromRoute] int id, [FromQuery] PlatformUserListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantUsersAsync(id, filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("tenants/{id:int}/organizations")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetOrganizations)]
    public async Task<IResult> GetTenantOrganizationsAsync([FromRoute] int id, [FromQuery] PlatformOrganizationListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetTenantOrganizationsAsync(id, filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("audit-logs")]
    [ModuleAuthorize(PermissionCodeConst.PlatformGetAuditLogs)]
    public async Task<IResult> GetAuditLogsAsync([FromQuery] PlatformAuditLogListFilter filter, CancellationToken ct = default)
    {
        var response = await _platformService.GetAuditLogsAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }
}
