namespace Application.Features.Acc.AccountingPeriods;

public interface IAccountingPeriodReadRepository
{
    Task<bool> HasOpenPreviousPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default);
    Task<bool> HasLaterClosedPeriodsAsync(int organizationId, DateOnly startDate, CancellationToken ct = default);
    Task<int> CountUnconfirmedDocumentsAsync(int organizationId, DateTime dateFrom, DateTime dateTo, CancellationToken ct = default);
    Task<bool> HasInvalidPostingBatchStateAsync(int organizationId, CancellationToken ct = default);
}
