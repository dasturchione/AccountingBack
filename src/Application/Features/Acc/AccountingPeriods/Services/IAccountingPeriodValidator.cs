using SharedKernel.Results;

namespace Application.Features.Acc.AccountingPeriods;

public interface IAccountingPeriodValidator
{
    Task<Result> EnsureOpenAsync(int organizationId, DateTime date, CancellationToken ct = default);
}
