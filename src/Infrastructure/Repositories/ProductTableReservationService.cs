using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Constants;

namespace Infrastructure.Repositories;

public class ProductTableReservationService : IProductTableReservationService
{
    private readonly AppDbContext _context;

    public ProductTableReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TryReserveAsync(IReadOnlyCollection<int> productTableIds, CancellationToken ct = default)
    {
        if (productTableIds.Count == 0)
            return true;

        var distinctIds = productTableIds.Distinct().ToList();

        var affectedRows = await _context.Set<ProductTable>()
            .Where(x => distinctIds.Contains(x.Id)
                        && x.StatusId == ProductTableStatusIdConst.IN_STOCK
                        && x.StateId == StateIdConst.ACTIVE)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.StatusId, ProductTableStatusIdConst.RESERVED), ct);

        return affectedRows == distinctIds.Count;
    }
}
