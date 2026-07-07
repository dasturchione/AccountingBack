using Domain.Entities;

namespace Application.Features.FaRevaluations;

public interface IFaRevaluationCommandRepository
{
    Task CreateAsync(FaRevaluationDoc entity, CancellationToken ct = default);
    Task UpdateAsync(FaRevaluationDoc entity, CancellationToken ct = default);
    Task DeleteLinesAsync(IEnumerable<FaRevaluationDocLine> entities, CancellationToken ct = default);
}
