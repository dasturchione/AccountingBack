using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Context;

public sealed class InventoryReadDbContext : IInventoryReadDbContext
{
    private readonly AppDbContext _context;

    public InventoryReadDbContext(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<ProductTable> ProductTables => _context.ProductTables;
    public IQueryable<Product> Products => _context.Products;
}
