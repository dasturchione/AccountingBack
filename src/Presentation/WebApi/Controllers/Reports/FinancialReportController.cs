using Application.Features.AccountingReports;
using Application.Features.Reports.Exports;
using Application.Features.Reports.FinancialReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers.Reports;

[Route("api/reports/financial")]
[ApiController]
[Authorize]
public sealed class FinancialReportController : ControllerBase
{
    private readonly IFinancialReportService _service;
    private readonly IReportExporter _exporter;

    public FinancialReportController(IFinancialReportService service, IReportExporter exporter)
    {
        _service = service;
        _exporter = exporter;
    }

    [HttpGet("balance-sheet")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> BalanceSheet([FromQuery] BalanceSheetFilter filter, CancellationToken ct = default)
        => (await _service.GetBalanceSheetAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("balance-sheet/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportBalanceSheet([FromQuery] BalanceSheetFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetBalanceSheetAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-balance-sheet", request.Format, ReportExportProfiles.FinancialBalanceSheet, [result.Value], ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("income-statement")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> IncomeStatement([FromQuery] IncomeStatementFilter filter, CancellationToken ct = default)
        => (await _service.GetIncomeStatementAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("income-statement/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportIncomeStatement([FromQuery] IncomeStatementFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetIncomeStatementAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-income-statement", request.Format, ReportExportProfiles.FinancialIncomeStatement, [result.Value], ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("cash-flow")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> CashFlow([FromQuery] CashFlowFilter filter, CancellationToken ct = default)
        => (await _service.GetCashFlowAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("cash-flow/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportCashFlow([FromQuery] CashFlowFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetCashFlowAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-cash-flow", request.Format, ReportExportProfiles.FinancialCashFlow, [result.Value], ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("turnover")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> Turnover([FromQuery] AccountTurnoverFilter filter, CancellationToken ct = default)
        => (await _service.GetAccountTurnoverAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("turnover/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportTurnover([FromQuery] AccountTurnoverFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAccountTurnoverAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-turnover", request.Format, ReportExportProfiles.FinancialTurnover, result.Value.Items, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("card")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> Card([FromQuery] AccountCardFilter filter, CancellationToken ct = default)
        => (await _service.GetAccountCardAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("card/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportCard([FromQuery] AccountCardFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetAccountCardAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-card", request.Format, ReportExportProfiles.FinancialCardTransactions, result.Value.Transactions, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("journal")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> Journal([FromQuery] JournalFilter filter, CancellationToken ct = default)
        => (await _service.GetJournalAsync(filter, ct)).Match(Results.Ok, CustomResults.Problem);

    [HttpGet("journal/export")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> ExportJournal([FromQuery] JournalFilter filter, [FromQuery] ReportExportRequestDto request, CancellationToken ct = default)
    {
        var result = await _service.GetJournalAsync(filter, ct);
        if (!result.IsSuccess)
            return CustomResults.Problem(result);

        var export = await _exporter.ExportAsync("financial-journal", request.Format, ReportExportProfiles.FinancialJournalEntries, result.Value.Entries, ct);
        return Results.File(export.Content, export.ContentType, export.FileName);
    }
}
