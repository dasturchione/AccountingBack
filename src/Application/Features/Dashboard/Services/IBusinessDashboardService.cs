using Application.Features.Dashboard.DTOs;

namespace Application.Features.Dashboard.Services;

public interface IBusinessDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(OverviewFilterDto filter, CancellationToken ct = default);
    Task<DashboardCashDto> GetCashAsync(CashFilterDto filter, CancellationToken ct = default);
    Task<DashboardReceivablesPayablesDto> GetReceivablesPayablesAsync(ReceivablesPayablesFilterDto filter, CancellationToken ct = default);
    Task<DashboardElectronicDocumentsDto> GetElectronicDocumentsAsync(ElectronicDocumentsFilterDto filter, CancellationToken ct = default);
    Task<DashboardTaxSummaryDto> GetTaxSummaryAsync(TaxSummaryFilterDto filter, CancellationToken ct = default);
}
