namespace Application.Abstractions.Integration.Edo;

public interface IEdoImportPreflightScheduler
{
    Task ScheduleAsync(long jobId, CancellationToken ct = default);
    Task ScheduleBulkImportAsync(long jobId, CancellationToken ct = default);
}
