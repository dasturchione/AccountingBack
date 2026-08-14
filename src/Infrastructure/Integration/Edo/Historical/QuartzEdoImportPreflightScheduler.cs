using Application.Abstractions.Integration.Edo;
using Infrastructure.BackgroundServices;
using Quartz;

namespace Integration.Edo.Historical;

public sealed class QuartzEdoImportPreflightScheduler(ISchedulerFactory schedulerFactory)
    : IEdoImportPreflightScheduler
{
    public async Task ScheduleAsync(long jobId, CancellationToken ct = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        await scheduler.TriggerJob(
            new JobKey(EdoImportPreflightJob.JobName),
            new JobDataMap { [EdoImportPreflightJob.JobIdKey] = jobId },
            ct);
    }

    public async Task ScheduleBulkImportAsync(long jobId, CancellationToken ct = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        await scheduler.TriggerJob(
            new JobKey(EdoBulkDraftImportJob.JobName),
            new JobDataMap { [EdoBulkDraftImportJob.JobIdKey] = jobId },
            ct);
    }
}
