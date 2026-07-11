using Application.Features.Manual;
using Domain.Entities;
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
public class ManualController : ControllerBase
{
    private readonly IManualService _manualService;

    public ManualController(IManualService manualService)
    {
        _manualService = manualService;
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetStates)]
    [HttpGet("states")]
    public async Task<IActionResult> GetStates(CancellationToken ct)
    {
        var result = await _manualService.GetStatesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetRegions)]
    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions(CancellationToken ct)
    {
        var result = await _manualService.GetRegionsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetDistricts)]
    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int? regionId, CancellationToken ct)
    {
        var result = await _manualService.GetDistrictsAsync(regionId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCurrencies)]
    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies(CancellationToken ct)
    {
        var result = await _manualService.GetCurrenciesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetUnits)]
    [HttpGet("units")]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        var result = await _manualService.GetUnitsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetDocumentStatuses)]
    [HttpGet("document-statuses")]
    public async Task<IActionResult> GetDocumentStatuses(CancellationToken ct)
    {
        var result = await _manualService.GetDocumentStatusesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCounterpartyTypes)]
    [HttpGet("counterparty-types")]
    public async Task<IActionResult> GetCounterpartyTypes(CancellationToken ct)
    {
        var result = await _manualService.GetCounterpartyTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPaymentTypes)]
    [HttpGet("payment-types")]
    public async Task<IActionResult> GetPaymentTypes(CancellationToken ct)
    {
        var result = await _manualService.GetPaymentTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetInventoryAdjustmentTypes)]
    [HttpGet("inventory-adjustment-types")]
    public async Task<IActionResult> GetInventoryAdjustmentTypes(CancellationToken ct)
    {
        var result = await _manualService.GetInventoryAdjustmentTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetFaGroups)]
    [HttpGet("fa-groups")]
    public async Task<IActionResult> GetFaGroups(CancellationToken ct)
    {
        var result = await _manualService.GetFaGroupsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetFaOkofs)]
    [HttpGet("fa-okofs")]
    public async Task<IActionResult> GetFaOkofs(CancellationToken ct)
    {
        var result = await _manualService.GetFaOkofsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetFaDepreciationMethods)]
    [HttpGet("fa-depreciation-methods")]
    public async Task<IActionResult> GetFaDepreciationMethods(CancellationToken ct)
    {
        var result = await _manualService.GetFaDepreciationMethodsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPostingAliases)]
    [HttpGet("posting-aliases")]
    public async Task<IActionResult> GetPostingAliases(CancellationToken ct)
    {
        var result = await _manualService.GetPostingAliasesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPaymentPurposes)]
    [HttpGet("payment-purposes")]
    public async Task<IActionResult> GetPaymentPurposes([FromQuery] short? operationTypeId, CancellationToken ct)
    {
        var result = await _manualService.GetPaymentPurposesAsync(operationTypeId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPriceRoundingMethods)]
    [HttpGet("price-rounding-methods")]
    public async Task<IActionResult> GetPriceRoundingMethods(CancellationToken ct)
    {
        var result = await _manualService.GetPriceRoundingMethodsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPricingMethods)]
    [HttpGet("pricing-methods")]
    public async Task<IActionResult> GetPricingMethods(CancellationToken ct)
    {
        var result = await _manualService.GetPricingMethodsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCostingMethods)]
    [HttpGet("costing-methods")]
    public async Task<IActionResult> GetCostingMethods(CancellationToken ct)
    {
        var result = await _manualService.GetCostingMethodsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetBanks)]
    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks(CancellationToken ct)
    {
        var result = await _manualService.GetBanksAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetDocumentTypes)]
    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken ct)
    {
        var result = await _manualService.GetDocumentTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetOperationTypes)]
    [HttpGet("operation-types")]
    public async Task<IActionResult> GetOperationTypes(CancellationToken ct)
    {
        var result = await _manualService.GetOperationTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetTaxTypes)]
    [HttpGet("tax-types")]
    public async Task<IActionResult> GetTaxTypes(CancellationToken ct)
    {
        var result = await _manualService.GetTaxTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetVatRates)]
    [HttpGet("vat-rates")]
    public async Task<IActionResult> GetVatRates(CancellationToken ct)
    {
        var result = await _manualService.GetVatRatesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetContractTypes)]
    [HttpGet("contract-types")]
    public async Task<IActionResult> GetContractTypes(CancellationToken ct)
    {
        var result = await _manualService.GetContractTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetRoles)]
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await _manualService.GetRolesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetUsers)]
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int? roleId, CancellationToken ct)
    {
        var result = await _manualService.GetUsersAsync(roleId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetOrganizations)]
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations(CancellationToken ct)
    {
        var result = await _manualService.GetOrganizationsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetBranches)]
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches(CancellationToken ct)
    {
        var result = await _manualService.GetBranchesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetDepartments)]
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetDepartmentsAsync(branchId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetPositions)]
    [HttpGet("positions")]
    public async Task<IActionResult> GetPositions(CancellationToken ct)
    {
        var result = await _manualService.GetPositionsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetContracts)]
    [HttpGet("contracts")]
    public async Task<IResult> GetContracts([FromQuery] int? counterpartyId = null, [FromQuery] short? contractTypeId = null, [FromQuery] DateTime? choosedDate = null, CancellationToken ct = default)
    {
        var result = await _manualService.GetContractsAsync(counterpartyId, contractTypeId, choosedDate, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCounterparties)]
    [HttpGet("counterparties")]
    public async Task<IActionResult> GetCounterparties(CancellationToken ct)
    {
        var result = await _manualService.GetCounterpartiesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetSuppliers)]
    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers(CancellationToken ct)
    {
        var result = await _manualService.GetSuppliersAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetClients)]
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients(CancellationToken ct)
    {
        var result = await _manualService.GetClientsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetProductGroups)]
    [HttpGet("product-groups")]
    public async Task<IActionResult> GetProductGroups(CancellationToken ct)
    {
        var result = await _manualService.GetProductGroupsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetProductTypes)]
    [HttpGet("product-types")]
    public async Task<IActionResult> GetProductTypes([FromQuery] bool? isService, CancellationToken ct)
    {
        var result = await _manualService.GetProductTypesAsync(isService, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetProducts)]
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? productGroupId,
        [FromQuery] int? warehouseId,
        [FromQuery] bool? isService,
        [FromQuery] short? productTypeId,
        [FromQuery] bool? isSold,
        [FromQuery] bool? isPurchased,
        CancellationToken ct)
    {
        var result = await _manualService.GetProductsAsync(productGroupId, warehouseId, isService, productTypeId, isSold, isPurchased, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetSourceProductTables)]
    [HttpGet("source-product-tables")]
    public async Task<IActionResult> GetSourceProductTables(CancellationToken ct)
    {
        var result = await _manualService.GetSourceProductTablesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetWarehouses)]
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses([FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetWarehousesAsync(branchId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetChartAccounts)]
    [HttpGet("chart-accounts")]
    public async Task<IActionResult> GetChartAccounts(CancellationToken ct)
    {
        var result = await _manualService.GetChartAccountsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetAccountingPolicies)]
    [HttpGet("accounting-policies")]
    public async Task<IActionResult> GetAccountingPolicies(CancellationToken ct)
    {
        var result = await _manualService.GetAccountingPoliciesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetAccountingPolicies)]
    [HttpGet("account-types")]
    public async Task<IActionResult> GetAccountTypes(CancellationToken ct)
    {
        var result = await _manualService.GetAccountTypesAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetOrgBankAccounts)]
    [HttpGet("org-bank-accounts")]
    public async Task<IActionResult> GetOrgBankAccounts(CancellationToken ct)
    {
        var result = await _manualService.GetOrgBankAccountsAsync(ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCounterpartyBankAccounts)]
    [HttpGet("counterparty-bank-accounts")]
    public async Task<IActionResult> GetCounterpartyBankAccounts([FromQuery] int? counterpartyId, [FromQuery] int? bankId, CancellationToken ct)
    {
        var result = await _manualService.GetCounterpartyBankAccountsAsync(counterpartyId, bankId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCashBoxes)]
    [HttpGet("cash-boxes")]
    public async Task<IActionResult> GetCashBoxes([FromQuery] int? branchId, CancellationToken ct)
    {
        var result = await _manualService.GetCashBoxesAsync(branchId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetCashOperations)]
    [HttpGet("cash-operations")]
    public async Task<IActionResult> GetCashOperations([FromQuery] int? cashBoxId, CancellationToken ct)
    {
        var result = await _manualService.GetCashOperationsAsync(cashBoxId, ct);
        return Ok(result);
    }

    [ModuleAuthorize(PermissionCodeConst.ManualGetLanguages)]
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages(CancellationToken ct)
    {
        var result = await _manualService.GetLanguagesAsync(ct);
        return Ok(result);
    }

    [AllowAnonymous]
    //[ModuleAuthorize(PermissionCodeConst.ManualGetModuleSubGroups)]
    [HttpGet("module-sub-groups")]
    public async Task<IActionResult> GetModuleSubGroups(CancellationToken ct)
    {
        var result = await _manualService.GetModuleSubGroupSelectListAsync(ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("subkonto-types")]
    public async Task<IActionResult> GetSubkontoTypes(CancellationToken ct)
    {
        var result = await _manualService.GetSubkontoTypesAsync(ct);
        return Ok(result);
    }
}
