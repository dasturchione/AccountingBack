namespace Application.Abstractions;

public interface IProductTableReservationService
{
    Task<bool> TryReserveAsync(int warehouseId, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default);
}
