using SharedKernel.Results;

namespace Application.Features.AccountingReports;

public interface IAccountingReportService
{
    Task<Result<BalanceSheetDto>> GetBalanceSheetAsync(BalanceSheetFilter filter, CancellationToken ct = default);
    Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(IncomeStatementFilter filter, CancellationToken ct = default);
    Task<Result<CashFlowDto>> GetCashFlowAsync(CashFlowFilter filter, CancellationToken ct = default);
    Task<Result<AccountTurnoverDto>> GetAccountTurnoverAsync(AccountTurnoverFilter filter, CancellationToken ct = default);
    Task<Result<AccountCardDto>> GetAccountCardAsync(AccountCardFilter filter, CancellationToken ct = default);
    Task<Result<JournalDto>> GetJournalAsync(JournalFilter filter, CancellationToken ct = default);
}
