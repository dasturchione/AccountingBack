using Application.Features.PurchaseDocs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public sealed class EdoBulkDraftImportJob(
    IEdoBulkDraftImportProcessor processor,
    ILogger<EdoBulkDraftImportJob> logger) : IJob
{
    public const string JobName = "EdoBulkDraftImportJob";
    public const string JobIdKey = "JobId";

    public async Task Execute(IJobExecutionContext context)
    {
        var workerId = $"{Environment.MachineName}:{context.FireInstanceId}";
        var ct = context.CancellationToken;
        var jobIds = context.MergedJobDataMap.ContainsKey(JobIdKey)
            ? new[] { context.MergedJobDataMap.GetLongValue(JobIdKey) }
            : await processor.GetRunnableBulkImportJobIdsAsync(ct);
        foreach (var jobId in jobIds)
        {
            try
            {
                await processor.ProcessBulkImportAsync(jobId, workerId, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError("EDO bulk Draft import worker failed; JobId={JobId}; ExceptionType={ExceptionType}",
                    jobId, exception.GetType().Name);
            }
        }
    }
}
