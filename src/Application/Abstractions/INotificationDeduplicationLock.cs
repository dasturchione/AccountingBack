namespace Application.Abstractions;

public interface INotificationDeduplicationLock
{
    Task AcquireAsync(string key, CancellationToken ct = default);
}
