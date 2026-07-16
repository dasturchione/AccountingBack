using Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class OrganizationSourceReader(AppDbContext context) : IOrganizationSourceReader
{
    public Task<int?> GetProductOrganizationIdAsync(int productId, CancellationToken ct = default) =>
        context.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => (int?)x.OrganizationId)
            .SingleOrDefaultAsync(ct);

    public Task<int?> GetCounterpartyOrganizationIdAsync(int counterpartyId, CancellationToken ct = default) =>
        context.CounterpartyCards.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == counterpartyId)
            .Select(x => (int?)x.OrganizationId)
            .SingleOrDefaultAsync(ct);

    public Task<int?> GetProductTableOrganizationIdAsync(int productTableId, CancellationToken ct = default) =>
        context.ProductTables.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == productTableId)
            .Select(x => (int?)x.Product.OrganizationId)
            .SingleOrDefaultAsync(ct);
}
