using Application.Features.PurchaseDocs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public sealed class EdoImportPreflightJob(
    IEdoImportPreflightProcessor processor,
    ILogger<EdoImportPreflightJob> logger) : IJob
{
    public const string JobName = "EdoImportPreflightJob";
    public const string JobIdKey = "JobId";

    public async Task Execute(IJobExecutionContext context)
    {
        var leaseOwner = $"{Environment.MachineName}:{context.FireInstanceId}";
        var cancellationToken = context.CancellationToken;

        if (context.MergedJobDataMap.ContainsKey(JobIdKey))
        {
            await processor.ProcessAsync(
                context.MergedJobDataMap.GetLongValue(JobIdKey),
                leaseOwner,
                cancellationToken);
            return;
        }

        var runnableJobIds = await processor.GetRunnableJobIdsAsync(cancellationToken);
        foreach (var jobId in runnableJobIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await processor.ProcessAsync(jobId, leaseOwner, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    "EDO import preflight recovery failed; JobId={JobId}; ExceptionType={ExceptionType}",
                    jobId,
                    exception.GetType().Name);
            }
        }
    }
}
