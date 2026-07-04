using Application.Features.InventoryCounts;
using Application.Features.Reports.Exports;
using Application.Features.Reports.WarehouseReports;
using Application.Features.WarehouseTransfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/warehouse")]
[ApiController]
[Authorize]
public sealed class WarehouseReportController : ControllerBase
{
    private readonly IWarehouseReportService _service;
    private readonly IReportExporter _exporter;

    public WarehouseReportController(IWarehouseReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("transfers")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferView)]
    public async Task<IResult> Transfers([FromQuery] WarehouseTransferListFilter filter, CancellationToken ct = default)
        => (await _service.GetTransfersAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("transfers/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferViewDetail)]
    public async Task<IResult> TransferById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetTransferAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("transfers/export")]
    [ModuleAuthorize(PermissionCodeConst.WarehouseTransferView)]
    public async Task<IResult> ExportTransfers([FromQuery] WarehouseTransferListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetTransfersAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("warehouse-transfers", request.Format, ReportExportProfiles.WarehouseTransfers, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("counts")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountView)]
    public async Task<IResult> Counts([FromQuery] InventoryCountListFilter filter, CancellationToken ct = default)
        => (await _service.GetCountsAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("counts/{id:long}")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountViewDetail)]
    public async Task<IResult> CountById([FromRoute] long id, CancellationToken ct = default)
        => (await _service.GetCountAsync(id, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("counts/export")]
    [ModuleAuthorize(PermissionCodeConst.InventoryCountView)]
    public async Task<IResult> ExportCounts([FromQuery] InventoryCountListFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetCountsAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("inventory-counts", request.Format, ReportExportProfiles.InventoryCounts, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
