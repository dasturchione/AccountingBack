using Application.Features.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/roles")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [ModuleAuthorize(PermissionCodeConst.RoleView)]
    public async Task<IResult> GetAllAsync([FromQuery] RoleListFilter filter, CancellationToken ct = default)
    {
        var response = await _roleService.GetAllAsync(filter, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.RoleViewDetail)]
    public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _roleService.GetByIdAsync(id, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPost]
    [ModuleAuthorize(PermissionCodeConst.RoleCreate)]
    public async Task<IResult> CreateAsync([FromBody] RoleCreateDto dto, CancellationToken ct = default)
    {
        var response = await _roleService.CreateAsync(dto, ct);
        return response.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpPut("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.RoleUpdate)]
    public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] RoleUpdateDto dto, CancellationToken ct = default)
    {
        var response = await _roleService.UpdateAsync(id, dto, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }

    [HttpDelete("{id:int}")]
    [ModuleAuthorize(PermissionCodeConst.RoleDelete)]
    public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
    {
        var response = await _roleService.DeleteAsync(id, ct);
        return response.Match(Results.NoContent, CustomResults.Problem);
    }
}
