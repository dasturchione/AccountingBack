using Application.Features.FaReceipts;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class FaReceiptCommandRepository : IFaReceiptCommandRepository
{
    private readonly AppDbContext _context;

    public FaReceiptCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaReceiptDoc entity, CancellationToken ct = default)
    {
        return _context.Set<FaReceiptDoc>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaReceiptDoc entity, CancellationToken ct = default)
    {
        _context.Set<FaReceiptDoc>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteLinesAsync(IEnumerable<FaReceiptDocLine> entities, CancellationToken ct = default)
    {
        _context.Set<FaReceiptDocLine>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}
