using Application.Features.Dashboard.DTOs;

namespace Application.Features.Dashboard.Services;

public interface IBusinessDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilterDto filter, CancellationToken ct = default);
    Task<DashboardCashDto> GetCashAsync(DashboardFilterDto filter, CancellationToken ct = default);
    Task<DashboardReceivablesPayablesDto> GetReceivablesPayablesAsync(DashboardFilterDto filter, CancellationToken ct = default);
    Task<DashboardElectronicDocumentsDto> GetElectronicDocumentsAsync(DashboardFilterDto filter, CancellationToken ct = default);
    Task<DashboardTaxSummaryDto> GetTaxSummaryAsync(DashboardFilterDto filter, CancellationToken ct = default);
}
