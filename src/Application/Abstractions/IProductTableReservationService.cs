namespace Application.Abstractions;

public interface IProductTableReservationService
{
    Task<bool> TryReserveAsync(IReadOnlyCollection<int> productTableIds, CancellationToken ct = default);
}
