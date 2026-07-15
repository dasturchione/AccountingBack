using Application.Abstractions;
using Application.Features.Inv.WarehouseProducts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProductTableReservationService : IProductTableReservationService
{
    private readonly AppDbContext _context;
    private readonly IWarehouseProductBalanceService _warehouseProductBalanceService;

    public ProductTableReservationService(
        AppDbContext context,
        IWarehouseProductBalanceService warehouseProductBalanceService)
    {
        _context = context;
        _warehouseProductBalanceService = warehouseProductBalanceService;
    }

    public async Task<bool> TryReserveAsync(int warehouseId, IReadOnlyCollection<int> productTableIds, CancellationToken ct = default)
    {
        if (productTableIds.Count == 0)
            return true;

        var ids = productTableIds.Distinct().ToList();
        if (ids.Count != productTableIds.Count)
            return false;

        var tables = await _context.Set<WarehouseProductTable>()
            .Include(x => x.ProductTable)
            .ThenInclude(x => x.Product)
            .Where(x => ids.Contains(x.ProductTableId))
            .ToListAsync(ct);

        if (tables.Count != ids.Count)
            return false;

        var items = tables
            .GroupBy(x => new { x.ProductTable.ProductId, x.ProductTable.Product.UnitId })
            .Select(x => new WarehouseProductBalanceItem(x.Key.ProductId, x.Key.UnitId, x.Count()))
            .ToList();

        var result = await _warehouseProductBalanceService.ReserveAsync(warehouseId, items, ids, ct);
        return result.IsSuccess;
    }
}