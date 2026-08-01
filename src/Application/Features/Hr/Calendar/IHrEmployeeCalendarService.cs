using SharedKernel.Results;

namespace Application.Features.Hr.Calendar;

public interface IHrEmployeeCalendarService
{
    Task<Result<HrEmployeeCalendarDto>> GetAsync(
        long employeeId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default);

    Task<Result<List<HrEmployeeCalendarDto>>> GetManyAsync(
        IReadOnlyCollection<long> employeeIds,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default);

    Task<Result<Dictionary<long, HrEmployeeCalendarSummaryDto>>> GetSummariesAsync(
        IReadOnlyCollection<long> employeeIds,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken ct = default);
}
