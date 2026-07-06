using Application.Features.FaAssets;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class FaAssetCommandRepository : IFaAssetCommandRepository
{
    private readonly AppDbContext _context;

    public FaAssetCommandRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task CreateAsync(FaAsset entity, CancellationToken ct = default)
    {
        return _context.Set<FaAsset>().AddAsync(entity, ct).AsTask();
    }

    public Task UpdateAsync(FaAsset entity, CancellationToken ct = default)
    {
        _context.Set<FaAsset>().Update(entity);
        return Task.CompletedTask;
    }
}
