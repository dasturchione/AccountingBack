using Application.Features.FaCommissionings;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaCommissioningCommandRepository : IFaCommissioningCommandRepository
{
    private readonly AppDbContext _context;

    public FaCommissioningCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(
        FaCommissioningDoc entity,
        CancellationToken ct = default) =>
        _context.Set<FaCommissioningDoc>().AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(
        FaCommissioningDoc entity,
        CancellationToken ct = default)
    {
        _context.Set<FaCommissioningDoc>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(
        IEnumerable<FaCommissioningDocLine> entities,
        CancellationToken ct = default)
    {
        _context.Set<FaCommissioningDocLine>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}
