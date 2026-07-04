using Application.Features.CounterpartyRegisterBalances;
using Application.Features.Reports.Exports;
using Application.Features.Reports.PayableReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/payable")]
[ApiController]
[Authorize]
public sealed class PayableReportController : ControllerBase
{
    private readonly IPayableReportService _service;
    private readonly IReportExporter _exporter;

    public PayableReportController(IPayableReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("balances")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceView)]
    public async Task<IResult> Balances([FromQuery] CounterpartyRegisterBalanceListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("balances/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceViewDetail)]
    public async Task<IResult> BalanceById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("balances/export")]
    [ModuleAuthorize(PermissionCodeConst.CounterpartyRegBalanceView)]
    public async Task<IResult> ExportBalances([FromQuery] CounterpartyRegisterBalanceListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("payable-balances", request.Format, ReportExportProfiles.CounterpartyBalances, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
