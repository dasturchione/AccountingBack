using Application.Features.Manual;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ManualController : ControllerBase
{
    private readonly IManualService _manualService;
    public ManualController(IManualService manualService)
    {
        _manualService = manualService;
    }

    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions(CancellationToken ct)
    {
        var result = await _manualService.GetRegionAsync(ct);
        return Ok(result);
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int? regionId, CancellationToken ct)
    {
        var result = await _manualService.GetDistrictAsync(regionId, ct);
        return Ok(result);
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates(CancellationToken ct)
    {
        var result = await _manualService.GetStateAsync(ct);
        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetRolesAsync(ct);
        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int? roleId, CancellationToken ct)
    {
        var result = await _manualService.GetUsersAsync(roleId, ct);
        return Ok(result);
    }
}
