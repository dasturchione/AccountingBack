namespace Application.Features.Contracts;

public interface IContractExpiryNotificationService
{
    Task<int> NotifyAsync(DateTime today, CancellationToken ct = default);
}
