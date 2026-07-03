using Application.Features.AccountingReports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

[Route("api/reports/accounting")]
[ApiController]
[Authorize]
public class AccountingReportController : ControllerBase
{
    private readonly IAccountingReportService _service;

    public AccountingReportController(IAccountingReportService service)
    {
        _service = service;
    }

    [HttpGet("balance-sheet")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetBalanceSheetAsync([FromQuery] BalanceSheetFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetBalanceSheetAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("income-statement")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetIncomeStatementAsync([FromQuery] IncomeStatementFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetIncomeStatementAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("cash-flow")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetCashFlowAsync([FromQuery] CashFlowFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetCashFlowAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("account-turnover")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetAccountTurnoverAsync([FromQuery] AccountTurnoverFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAccountTurnoverAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("account-card")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetAccountCardAsync([FromQuery] AccountCardFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetAccountCardAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }

    [HttpGet("journal")]
    [ModuleAuthorize(PermissionCodeConst.AccRegEntryView)]
    public async Task<IResult> GetJournalAsync([FromQuery] JournalFilter filter, CancellationToken ct = default)
    {
        var result = await _service.GetJournalAsync(filter, ct);
        return result.Match(Results.Ok, CustomResults.Problem);
    }
}
