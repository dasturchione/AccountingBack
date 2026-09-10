using Domain.Entities;

namespace Application.Features.Pay.PayrollDocuments;

/// <summary>
/// Resolves the employment that is effective for each day in a payroll period.
/// When records overlap, the latest effective start date wins (and the highest
/// id is used as a deterministic tie-breaker). Days without an active
/// employment remain gaps and are not silently assigned to another record.
/// </summary>
public static class PayrollEmploymentSegmentCalculator
{
    public static IReadOnlyList<PayrollEmploymentSegment> Split(
        PayPeriod period,
        IEnumerable<PayEmployment> employments)
    {
        ArgumentNullException.ThrowIfNull(period);
        ArgumentNullException.ThrowIfNull(employments);
        if (period.EndDate < period.StartDate)
            throw new ArgumentException("Payroll period end date cannot precede its start date.", nameof(period));

        var candidates = employments
            .Where(x => x.StartDate <= period.EndDate &&
                        (!x.EndDate.HasValue || x.EndDate.Value >= period.StartDate))
            .ToList();
        var segments = new List<PayrollEmploymentSegment>();
        PayrollEmploymentSegment? current = null;

        for (var date = period.StartDate; date <= period.EndDate; date = date.AddDays(1))
        {
            var employment = candidates
                .Where(x => x.StartDate <= date &&
                            (!x.EndDate.HasValue || x.EndDate.Value >= date))
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            if (employment is null)
            {
                FlushCurrent();
                continue;
            }

            if (current is not null &&
                current.EmploymentId == employment.Id &&
                current.EndDate.AddDays(1) == date)
            {
                current = current with { EndDate = date };
                continue;
            }

            FlushCurrent();
            current = new PayrollEmploymentSegment(
                employment.Id,
                employment.EmployeeId,
                date,
                date,
                employment.MonthlySalary,
                employment.EmploymentRate,
                employment.WeeklyHours,
                employment.CurrencyId,
                employment.ExpenseAccountId);
        }

        FlushCurrent();
        return segments;

        void FlushCurrent()
        {
            if (current is not null)
                segments.Add(current);
            current = null;
        }
    }
}

public sealed record PayrollEmploymentSegment(
    long EmploymentId,
    long EmployeeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal MonthlySalary,
    decimal EmploymentRate,
    decimal WeeklyHours,
    short CurrencyId,
    int? ExpenseAccountId);
