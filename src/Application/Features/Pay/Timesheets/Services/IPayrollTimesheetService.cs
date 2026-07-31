using Application.Common.Pagination;
using Application.Features.Hr.Calendar;
using SharedKernel.Results;

namespace Application.Features.Pay.Timesheets;

public interface IPayrollTimesheetService
{
    Task<Result<PagedResponse<PayrollTimesheetListDto>>> GetAllAsync(PayrollTimesheetListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollTimesheetDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<HrEmployeeCalendarDto>> GetEmployeeCalendarAsync(long periodId, long employeeId, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PayrollTimesheetCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PayrollTimesheetUpdateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
