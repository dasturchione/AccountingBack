using Application.Features.Contracts;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel.Time;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public sealed class ContractExpiryNotificationJob : IJob
{
    private readonly IContractExpiryNotificationService _service;
    private readonly ILogger<ContractExpiryNotificationJob> _logger;

    public ContractExpiryNotificationJob(
        IContractExpiryNotificationService service,
        ILogger<ContractExpiryNotificationJob> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var today = TashkentTime.Today;
            _logger.LogInformation("ContractExpiryNotificationJob started for {Date}", today);
            var created = await _service.NotifyAsync(today, context.CancellationToken);
            _logger.LogInformation("ContractExpiryNotificationJob finished for {Date}; CreatedCount={CreatedCount}", today, created);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("ContractExpiryNotificationJob failed; ExceptionType={ExceptionType}", ex.GetType().Name);
        }
    }
}
