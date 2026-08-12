using Domain.Entities;

namespace Application.Features.FaCommissionings;

public interface IFaCommissioningCommandRepository
{
    Task CreateAsync(FaCommissioningDoc entity, CancellationToken ct = default);
    Task UpdateAsync(FaCommissioningDoc entity, CancellationToken ct = default);
    Task DeleteLinesAsync(
        IEnumerable<FaCommissioningDocLine> entities,
        CancellationToken ct = default);
}
