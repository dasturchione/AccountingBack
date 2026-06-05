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

    // sys
    Task<List<SelectListDto>> GetRolesAsync(CancellationToken ct = default);
    Task<List<SelectListDto>> GetUsersAsync(int? roleId = null, CancellationToken ct = default);
}
