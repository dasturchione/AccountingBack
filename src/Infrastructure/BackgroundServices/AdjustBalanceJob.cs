using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices;

/// <summary>
/// Har kuni avtomatik ravishda balanslarni tekshirib tuzatadigan job.
/// </summary>
[DisallowConcurrentExecution]
public class AdjustBalanceJob : IJob
{
    private readonly ILogger<AdjustBalanceJob> _logger;

    public AdjustBalanceJob(ILogger<AdjustBalanceJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("AdjustBalanceJob boshlandi: {Time}", DateTime.Now);

            // TODO: balans tuzatish logikasini shu yerga qo'shing
            await Task.CompletedTask;

            _logger.LogInformation("AdjustBalanceJob yakunlandi: {Time}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AdjustBalanceJob xatosi: {Message}", ex.Message);
        }
    }
}
