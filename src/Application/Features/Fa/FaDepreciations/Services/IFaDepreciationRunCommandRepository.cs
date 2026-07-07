using Domain.Entities;

namespace Application.Features.FaDepreciations;

public interface IFaDepreciationRunCommandRepository
{
    Task CreateAsync(FaDepreciationRun entity, CancellationToken ct = default);
    Task UpdateAsync(FaDepreciationRun entity, CancellationToken ct = default);
}
