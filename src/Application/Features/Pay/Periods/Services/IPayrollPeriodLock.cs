namespace Application.Features.Pay.Periods;

public interface IPayrollPeriodLock
{
    Task AcquireAsync(long periodId, CancellationToken ct = default);
}
