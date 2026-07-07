using Domain.Entities;

namespace Application.Features.FaMovements;

public interface IFaMovementCommandRepository
{
    Task CreateAsync(FaMovementDoc entity, CancellationToken ct = default);
    Task UpdateAsync(FaMovementDoc entity, CancellationToken ct = default);
    Task DeleteLinesAsync(IEnumerable<FaMovementDocLine> entities, CancellationToken ct = default);
}
