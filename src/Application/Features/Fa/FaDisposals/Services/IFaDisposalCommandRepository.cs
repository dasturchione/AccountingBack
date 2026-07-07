using Domain.Entities;

namespace Application.Features.FaDisposals;

public interface IFaDisposalCommandRepository
{
    Task CreateAsync(FaDisposalDoc entity, CancellationToken ct = default);
    Task UpdateAsync(FaDisposalDoc entity, CancellationToken ct = default);
    Task DeleteLinesAsync(IEnumerable<FaDisposalDocLine> entities, CancellationToken ct = default);
}
