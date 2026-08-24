using Application.Abstractions.Integration.Edo;

namespace Integration.Edo.Historical;

public sealed class DisabledEdoImportPreflightScheduler : IEdoImportPreflightScheduler
{
    public Task ScheduleAsync(long jobId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task ScheduleBulkImportAsync(long jobId, CancellationToken ct = default)
        => Task.CompletedTask;
}
