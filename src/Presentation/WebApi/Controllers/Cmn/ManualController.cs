using Application.Features.Manual;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/manuals")]
[ApiController]
[Authorize]
[ModuleAuthorize(PermissionCodeConst.ManualView)]
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

    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks(CancellationToken ct)
    {
        var result = await _manualService.GetBanksAsync(ct);
        return Ok(result);
    }

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken ct)
    {
        var result = await _manualService.GetDocumentTypesAsync(ct);
        return Ok(result);
    }

    [HttpGet("operation-types")]
    public async Task<IActionResult> GetOperationTypes(CancellationToken ct)
    {
        var result = await _manualService.GetOperationTypesAsync(ct);
        return Ok(result);
    }

    [HttpGet("tax-types")]
    public async Task<IActionResult> GetTaxTypes(CancellationToken ct)
    {
        var result = await _manualService.GetTaxTypesAsync(ct);
        return Ok(result);
    }

    [HttpGet("vat-rates")]
    public async Task<IActionResult> GetVatRates(CancellationToken ct)
    {
        var result = await _manualService.GetVatRatesAsync(ct);
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

    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations(CancellationToken ct)
    {
        var result = await _manualService.GetOrganizationsAsync(ct);
        return Ok(result);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetBranchesAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] int? organizationId, [FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetDepartmentsAsync(organizationId, branchId, ct);
        return Ok(result);
    }

    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetPositionsAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("contracts")]
    public async Task<IResult> GetContracts([FromQuery] int? counterpartyId = null, [FromQuery] DateTime? choosedDate = null, CancellationToken ct = default)
    {
        var result = await _manualService.GetContractsAsync(counterpartyId, choosedDate, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("counterparties")]
    public async Task<IActionResult> GetCounterparties([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetCounterpartiesAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("product-groups")]
    public async Task<IActionResult> GetProductGroups([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetProductGroupsAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] int? organizationId, [FromQuery] int? productGroupId, CancellationToken ct)
    {
        var result = await _manualService.GetProductsAsync(organizationId, productGroupId, ct);
        return Ok(result);
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses([FromQuery] int? organizationId, [FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetWarehousesAsync(organizationId, branchId, ct);
        return Ok(result);
    }

    [HttpGet("chart-accounts")]
    public async Task<IActionResult> GetChartAccounts([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetChartAccountsAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("org-bank-accounts")]
    public async Task<IActionResult> GetOrgBankAccounts([FromQuery] int? organizationId, CancellationToken ct)
    {
        var result = await _manualService.GetOrgBankAccountsAsync(organizationId, ct);
        return Ok(result);
    }

    [HttpGet("cash-boxes")]
    public async Task<IActionResult> GetCashBoxes([FromQuery] int? organizationId, [FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetCashBoxesAsync(organizationId, branchId, ct);
        return Ok(result);
    }

    [HttpGet("cash-operations")]
    public async Task<IActionResult> GetCashOperations([FromQuery] int? organizationId, [FromQuery] int? cashBoxId, CancellationToken ct)
    {
        var result = await _manualService.GetCashOperationsAsync(organizationId, cashBoxId, ct);
        return Ok(result);
    }

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages(CancellationToken ct)
    {
        var result = await _manualService.GetLanguagesAsync(ct);
        return Ok(result);
    }

    [HttpGet("module-sub-groups")]
    public async Task<IActionResult> GetModuleSubGroups(CancellationToken ct)
    {
        var result = await _manualService.GetModuleSubGroupSelectListAsync(ct);
        return Ok(result);
    }
}
