using Application.Abstractions;
using Application.Features.Cmn.AslBelgi.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Integration.AslBelgi.Services;

/// <summary>EF-backed read port for the marking flow (see <see cref="IAslBelgiMarkingRepository"/>).</summary>
public sealed class AslBelgiMarkingReadRepository : IAslBelgiMarkingRepository
{
    private readonly IInventoryReadDbContext _readDbContext;

    public AslBelgiMarkingReadRepository(IInventoryReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<AslBelgiProductMarkingInfo?> GetProductForMarkingAsync(int productId, int organizationId, CancellationToken ct = default)
    {
        return await _readDbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && p.OrganizationId == organizationId)
            .Select(p => new AslBelgiProductMarkingInfo(p.Id, p.Gtin))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<string>> GetExistingMarkingsAsync(int organizationId, IReadOnlyCollection<string> markings, CancellationToken ct = default)
    {
        if (markings.Count == 0)
            return [];

        return await _readDbContext.ProductTables
            .AsNoTracking()
            .Where(pt => pt.OrganizationId == organizationId
                         && pt.MarkingNumber != null
                         && markings.Contains(pt.MarkingNumber))
            .Select(pt => pt.MarkingNumber!)
            .ToListAsync(ct);
    }
}
