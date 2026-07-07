using Application.Features.BankOperations;
using Application.Features.Reports.Exports;
using Application.Features.Reports.BankReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/bank")]
[ApiController]
[Authorize]
public sealed class BankReportController : ControllerBase
{
    private readonly IBankReportService _service;
    private readonly IReportExporter _exporter;

    public BankReportController(IBankReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("operations")]
    [ModuleAuthorize(PermissionCodeConst.BankReportOperations)]
    public async Task<IResult> Operations([FromQuery] BankOperationListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("operations/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.BankReportOperationById)]
    public async Task<IResult> OperationById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("operations/export")]
    [ModuleAuthorize(PermissionCodeConst.BankReportExportOperations)]
    public async Task<IResult> ExportOperations([FromQuery] BankOperationListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("bank-operations", request.Format, ReportExportProfiles.BankOperations, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
