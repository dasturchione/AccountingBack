using Application.Features.FaMovements;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaMovementCommandRepository : IFaMovementCommandRepository
{
    private readonly AppDbContext _context;

    public FaMovementCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaMovementDoc entity, CancellationToken ct = default)
    {
        return _context.Set<FaMovementDoc>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaMovementDoc entity, CancellationToken ct = default)
    {
        _context.Set<FaMovementDoc>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(IEnumerable<FaMovementDocLine> entities, CancellationToken ct = default)
    {
        _context.Set<FaMovementDocLine>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}
