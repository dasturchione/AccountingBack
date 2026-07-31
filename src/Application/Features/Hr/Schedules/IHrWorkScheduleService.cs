using SharedKernel.Results;

namespace Application.Features.Hr.Schedules;

public interface IHrWorkScheduleService
{
    Task<Result<List<HrWorkScheduleDto>>> GetAllAsync(long employeeId, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(long employeeId, HrWorkScheduleSaveDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long employeeId, long scheduleId, HrWorkScheduleSaveDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long employeeId, long scheduleId, CancellationToken ct = default);
}
