using Application.Features.Manual;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/manual")]
[ApiController]
[Authorize]
public class ManualController : ControllerBase
{
    private readonly IManualService _manualService;

    public ManualController(IManualService manualService)
    {
        _manualService = manualService;
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates(CancellationToken ct)
    {
        var result = await _manualService.GetStatesAsync(ct);
        return Ok(result);
    }

    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions(CancellationToken ct)
    {
        var result = await _manualService.GetRegionsAsync(ct);
        return Ok(result);
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int? regionId, CancellationToken ct)
    {
        var result = await _manualService.GetDistrictsAsync(regionId, ct);
        return Ok(result);
    }

    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies(CancellationToken ct)
    {
        var result = await _manualService.GetCurrenciesAsync(ct);
        return Ok(result);
    }

    [HttpGet("units")]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        var result = await _manualService.GetUnitsAsync(ct);
        return Ok(result);
    }

    [HttpGet("document-statuses")]
    public async Task<IActionResult> GetDocumentStatuses(CancellationToken ct)
    {
        var result = await _manualService.GetDocumentStatusesAsync(ct);
        return Ok(result);
    }

    [HttpGet("counterparty-types")]
    public async Task<IActionResult> GetCounterpartyTypes(CancellationToken ct)
    {
        var result = await _manualService.GetCounterpartyTypesAsync(ct);
        return Ok(result);
    }

    [HttpGet("payment-types")]
    public async Task<IActionResult> GetPaymentTypes(CancellationToken ct)
    {
        var result = await _manualService.GetPaymentTypesAsync(ct);
        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
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
