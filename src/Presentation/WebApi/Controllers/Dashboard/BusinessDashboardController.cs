using Application.Features.Dashboard.DTOs;
using Application.Features.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;

namespace WebApi.Controllers.Dashboard;

[Route("api/dashboard")]
[ApiController]
[Authorize]
[ReadOnlyModuleAuthorize(PermissionCodeConst.DashboardView)]
public sealed class BusinessDashboardController : ControllerBase
{
    private readonly IBusinessDashboardService _service;

    public BusinessDashboardController(IBusinessDashboardService service) => _service = service;

    [HttpGet("overview")]
    public Task<DashboardOverviewDto> GetOverview([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetOverviewAsync(filter, ct);

    [HttpGet("cash")]
    public Task<DashboardCashDto> GetCash([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetCashAsync(filter, ct);

    [HttpGet("receivables-payables")]
    public Task<DashboardReceivablesPayablesDto> GetReceivablesPayables([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetReceivablesPayablesAsync(filter, ct);

    [HttpGet("electronic-documents")]
    public Task<DashboardElectronicDocumentsDto> GetElectronicDocuments([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetElectronicDocumentsAsync(filter, ct);

    [HttpGet("tax-summary")]
    public Task<DashboardTaxSummaryDto> GetTaxSummary([FromQuery] DashboardFilterDto filter, CancellationToken ct = default) =>
        _service.GetTaxSummaryAsync(filter, ct);
}
