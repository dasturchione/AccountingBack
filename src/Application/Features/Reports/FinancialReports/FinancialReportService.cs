using Application.Features.AccountingReports;
using SharedKernel.Results;

namespace Application.Features.Reports.FinancialReports;

public interface IFinancialReportService
{
    Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(BalanceSheetFilter filter, CancellationToken ct = default);
    Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementFilter filter, CancellationToken ct = default);
    Task<Result<CashFlowDto>> GetCashFlowAsync(CashFlowFilter filter, CancellationToken ct = default);
    Task<Result<AccountTurnoverDto>> GetAccountTurnoverAsync(AccountTurnoverFilter filter, CancellationToken ct = default);
    Task<Result<AccountCardDto>> GetAccountCardAsync(AccountCardFilter filter, CancellationToken ct = default);
    Task<Result<JournalDto>> GetJournalAsync(JournalFilter filter, CancellationToken ct = default);
}

public sealed class FinancialReportService : IFinancialReportService
{
    private readonly IAccountingReportService _inner;

    public FinancialReportService(IAccountingReportService inner)
    {
        _inner = inner;
    }

    public Task<Result<AccountCardDto>> GetAccountCardAsync(AccountCardFilter filter, CancellationToken ct = default) =>
        _inner.GetAccountCardAsync(filter, ct);

    public Task<Result<AccountTurnoverDto>> GetAccountTurnoverAsync(AccountTurnoverFilter filter, CancellationToken ct = default) =>
        _inner.GetAccountTurnoverAsync(filter, ct);

    public Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(BalanceSheetFilter filter, CancellationToken ct = default) =>
        _inner.GetBalanceSheetAsync(filter, ct);

    public Task<Result<CashFlowDto>> GetCashFlowAsync(CashFlowFilter filter, CancellationToken ct = default) =>
        _inner.GetCashFlowAsync(filter, ct);

    public Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementFilter filter, CancellationToken ct = default) =>
        _inner.GetIncomeStatementAsync(filter, ct);

    public Task<Result<JournalDto>> GetJournalAsync(JournalFilter filter, CancellationToken ct = default) =>
        _inner.GetJournalAsync(filter, ct);
}
