using Application.Features.Rnt.RentalAccruals;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel.Time;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public sealed class RentalAccrualJob(
    IRentalAccrualGenerationService generationService,
    ILogger<RentalAccrualJob> logger) : IJob
{
    public const string CronSchedule = "0 0 0 1 * ?";

    public async Task Execute(IJobExecutionContext context)
    {
        await ExecuteAsync(TashkentTime.Today, context.CancellationToken);
    }

    internal async Task ExecuteAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var accrualMonth = RentalAccrualSchedule.GetPreviousMonth(date);
            var result = await generationService.GenerateDueAsync(
                accrualMonth.Year,
                accrualMonth.Month,
                organizationId: null,
                cancellationToken);
            if (!result.IsSuccess)
            {
                logger.LogError(
                    "Rental accrual generation failed for {Year}-{Month}: {ErrorCode}",
                    accrualMonth.Year,
                    accrualMonth.Month,
                    result.Error.Code);
                throw new JobExecutionException($"Rental accrual generation failed: {result.Error.Code}");
            }

            logger.LogInformation(
                "Rental accrual generation finished for {Year}-{Month}; Documents={DocumentCount}; Items={ItemCount}",
                accrualMonth.Year,
                accrualMonth.Month,
                result.Value.CreatedDocumentCount,
                result.Value.CreatedItemCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rental accrual generation crashed for {Date}", date);
            throw;
        }
    }
}
