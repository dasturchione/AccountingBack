using SharedKernel.Results;

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
    Task<List<SelectListDto>> GetInventoryAdjustmentTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetFaGroupsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetFaOkofsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetFaDepreciationMethodsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetFaReceiptTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetFaDisposalTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetPriceRoundingMethodsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetPricingMethodsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetCostingMethodsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetBanksAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDocumentTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetOperationTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetTaxTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetVatRatesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetContractTypesAsync(CancellationToken ct = default);

    // sys
    Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default);
    Task<List<ModuleSubGroupSelectListDto>> GetModuleSubGroupSelectListAsync(CancellationToken ct = default);

    // org
    Task<List<SelectListDto>> GetOrganizationsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetBranchesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetDepartmentsAsync(int? branchId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetPositionsAsync(CancellationToken ct = default);

    // contracts
    Task<Result<List<SelectListDto>>> GetContractsAsync(int? counterpartyId = null, short? contractTypeId = null, DateTime? choosedDate = null, CancellationToken ct = default);

    // counterparty
    Task<List<SelectListDto>> GetCounterpartiesAsync(CancellationToken ct = default);
    Task<List<CounterpartySelectListDto>> GetSuppliersAsync(CancellationToken ct = default);
    Task<List<CounterpartySelectListDto>> GetClientsAsync(CancellationToken ct = default);

    // inv
    Task<List<SelectListDto>> GetProductGroupsAsync(CancellationToken ct = default);
    Task<List<ProductSelectListDto>> GetProductsAsync(
        int? productGroupId = null,
        int? warehouseId = null,
        bool? isService = null,
        bool? isSold = null,
        bool? isPurchased = null,
        CancellationToken ct = default);
    Task<List<SelectListDto>> GetWarehousesAsync(int? branchId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetSourceProductTablesAsync(CancellationToken ct = default);

    // acc
    Task<List<SelectListDto>> GetSubkontoTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetAccountTypesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetAccountingPoliciesAsync(CancellationToken ct = default);
    Task<List<ChartAccountSelectListDto>> GetChartAccountsAsync(CancellationToken ct = default);

    // bank
    Task<List<SelectListDto>> GetOrgBankAccountsAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetCounterpartyBankAccountsAsync(int? counterpartyId = null, int? bankId = null, CancellationToken ct = default);

    // cash
    Task<List<SelectListDto>> GetCashBoxesAsync(int? branchId = null, CancellationToken ct = default);
    Task<List<SelectListDto>> GetCashOperationsAsync(int? cashBoxId = null, CancellationToken ct = default);

    // languages
    Task<List<SelectListDto>> GetLanguagesAsync(CancellationToken ct = default);
}
