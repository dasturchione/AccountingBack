using Application.Features.CashOperations;
using Application.Features.Reports.Exports;
using Application.Features.Reports.CashReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/cash")]
[ApiController]
[Authorize]
public sealed class CashReportController : ControllerBase
{
    private readonly ICashReportService _service;
    private readonly IReportExporter _exporter;

    public CashReportController(ICashReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("operations")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationView)]
    public async Task<IResult> Operations([FromQuery] CashOperationListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("operations/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationViewDetail)]
    public async Task<IResult> OperationById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("operations/export")]
    [ModuleAuthorize(PermissionCodeConst.CashOperationView)]
    public async Task<IResult> ExportOperations([FromQuery] CashOperationListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("cash-operations", request.Format, ReportExportProfiles.CashOperations, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
