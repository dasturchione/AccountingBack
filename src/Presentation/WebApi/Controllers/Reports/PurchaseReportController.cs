using Application.Features.PurchaseDocs;
using Application.Features.Reports.PurchaseReports;
using Application.Features.Reports.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/purchase")]
[ApiController]
[Authorize]
public sealed class PurchaseReportController : ControllerBase
{
    private readonly IPurchaseReportService _service;
    private readonly IReportExporter _exporter;

    public PurchaseReportController(IPurchaseReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("documents")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseReportGetAll)]
    public async Task<IResult> GetAllAsync([FromQuery] PurchaseDocListFilter filter, CancellationToken ct = default)
        => (await _service.GetAllAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseReportGetById)]
    public async Task<IResult> GetByIdAsync([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetByIdAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("documents/export")]
    [ModuleAuthorize(PermissionCodeConst.PurchaseReportExport)]
    public async Task<IResult> ExportAsync([FromQuery] PurchaseDocListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("purchase-documents", request.Format, ReportExportProfiles.PurchaseDocs, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
