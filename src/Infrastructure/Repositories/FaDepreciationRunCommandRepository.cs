using Application.Features.FaDepreciations;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaDepreciationRunCommandRepository : IFaDepreciationRunCommandRepository
{
    private readonly AppDbContext _context;

    public FaDepreciationRunCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaDepreciationRun entity, CancellationToken ct = default)
    {
        return _context.Set<FaDepreciationRun>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaDepreciationRun entity, CancellationToken ct = default)
    {
        _context.Set<FaDepreciationRun>().Update(entity);
        return Task.CompletedTask;
    }
}
