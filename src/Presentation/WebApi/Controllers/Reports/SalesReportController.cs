using Application.Features.Reports.SalesReports;
using Application.Features.SaleDocs;
using Application.Features.Reports.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/sales")]
[ApiController]
[Authorize]
public sealed class SalesReportController : ControllerBase
{
    private readonly ISalesReportService _service;
    private readonly IReportExporter _exporter;

    public SalesReportController(ISalesReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("documents")]
    [ModuleAuthorize(PermissionCodeConst.SalesReportGetAll)]
    public async Task<IResult> GetAllAsync([FromQuery] SaleDocListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.SalesReportGetById)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents/export")]
    [ModuleAuthorize(PermissionCodeConst.SalesReportExport)]
    public async Task<IResult> ExportAsync([FromQuery] SaleDocListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync(
            "sales-documents",
            request.Format,
            ReportExportProfiles.SaleDocs,
            result.Value.Items,
            ct);

        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
