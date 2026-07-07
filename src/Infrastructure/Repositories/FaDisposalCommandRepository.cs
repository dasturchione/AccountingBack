using Application.Features.FaDisposals;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaDisposalCommandRepository : IFaDisposalCommandRepository
{
    private readonly AppDbContext _context;

    public FaDisposalCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaDisposalDoc entity, CancellationToken ct = default)
    {
        return _context.Set<FaDisposalDoc>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaDisposalDoc entity, CancellationToken ct = default)
    {
        _context.Set<FaDisposalDoc>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(IEnumerable<FaDisposalDocLine> entities, CancellationToken ct = default)
    {
        _context.Set<FaDisposalDocLine>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}
