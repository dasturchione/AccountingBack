namespace Application.Features.AccountingReports;

public interface IAccountingReportReadRepository
{
    Task<CashFlowReadResult> GetCashFlowAsync(CashFlowReadRequest request, CancellationToken ct = default);
    Task<JournalReadResult> GetJournalAsync(JournalReadRequest request, CancellationToken ct = default);
}
