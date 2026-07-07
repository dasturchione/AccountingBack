using Application.Features.FaRevaluations;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaRevaluationCommandRepository : IFaRevaluationCommandRepository
{
    private readonly AppDbContext _context;

    public FaRevaluationCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaRevaluationDoc entity, CancellationToken ct = default)
    {
        return _context.Set<FaRevaluationDoc>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaRevaluationDoc entity, CancellationToken ct = default)
    {
        _context.Set<FaRevaluationDoc>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(IEnumerable<FaRevaluationDocLine> entities, CancellationToken ct = default)
    {
        _context.Set<FaRevaluationDocLine>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}
