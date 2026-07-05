using Application.Features.Notifications;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.BackgroundServices;

[DisallowConcurrentExecution]
public sealed class NotificationEmailDispatchJob : IJob
{
    private readonly INotificationEmailDispatcher _dispatcher;
    private readonly ILogger<NotificationEmailDispatchJob> _logger;

    public NotificationEmailDispatchJob(
        INotificationEmailDispatcher dispatcher,
        ILogger<NotificationEmailDispatchJob> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("NotificationEmailDispatchJob started at {Time}", DateTime.Now);
            await _dispatcher.DispatchPendingAsync(context.CancellationToken);
            _logger.LogInformation("NotificationEmailDispatchJob finished at {Time}", DateTime.Now);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotificationEmailDispatchJob failed: {Message}", ex.Message);
        }
    }
}
