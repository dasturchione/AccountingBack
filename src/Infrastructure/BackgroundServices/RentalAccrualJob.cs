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
    public async Task Execute(IJobExecutionContext context)
    {
        await ExecuteAsync(TashkentTime.Today, context.CancellationToken);
    }

    internal async Task ExecuteAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            var result = await generationService.GenerateDueAsync(date.Date, organizationId: null, cancellationToken);
            if (!result.IsSuccess)
            {
                logger.LogError("Rental accrual generation failed for {Date}: {ErrorCode}", date, result.Error.Code);
                throw new JobExecutionException($"Rental accrual generation failed: {result.Error.Code}");
            }

            logger.LogInformation(
                "Rental accrual generation finished for {Date}; Documents={DocumentCount}; Items={ItemCount}",
                date,
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
