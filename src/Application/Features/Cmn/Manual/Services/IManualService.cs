namespace Application.Features.Manual;

public interface IManualService
{
    // cmn
    Task<List<SelectListDto>> GetStatesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetRegionsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDistrictsAsync(int? regionId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetCurrenciesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetUnitsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDocumentStatusesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetCounterpartyTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetPaymentTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetBanksAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDocumentTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetOperationTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetTaxTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetVatRatesAsync(CancellationToken ct = default);

    // sys
    Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default);
    Task<List<ModuleSubGroupSelectListDto>> GetModuleSubGroupSelectListAsync(CancellationToken ct = default);

    // org
    Task<List<SelectListDto>> GetBranchesAsync(int? organizationId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetDepartmentsAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetPositionsAsync(int? organizationId = null, CancellationToken ct = default);

    // counterparty
    Task<List<SelectListDto>> GetCounterpartiesAsync(int? organizationId = null, CancellationToken ct = default);

    // inv
    Task<List<SelectListDto>> GetProductGroupsAsync(int? organizationId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetProductsAsync(int? organizationId = null, int? productGroupId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetWarehousesAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default);

    // acc
    Task<List<SelectListDto>> GetChartAccountsAsync(int? organizationId = null, CancellationToken ct = default);

    // bank
    Task<List<SelectListDto>> GetOrgBankAccountsAsync(int? organizationId = null, CancellationToken ct = default);

    // cash
    Task<List<SelectListDto>> GetCashBoxesAsync(int? organizationId = null, int? branchId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetCashOperationsAsync(int? organizationId = null, int? cashBoxId = null, CancellationToken ct = default);

    // languages
    Task<List<SelectListDto>> GetLanguagesAsync(CancellationToken ct = default);
}
