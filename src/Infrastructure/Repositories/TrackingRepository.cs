using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class TrackingRepository<TEntity>(AppDbContext context) : ITrackingRepository<TEntity> where TEntity : class
{
    public Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        context.Set<TEntity>().AddAsync(entity, ct).AsTask();

    public Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        context.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }
}
