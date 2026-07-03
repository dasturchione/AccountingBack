using Domain.Entities;

namespace Application.Abstractions;

public interface IInventoryReadDbContext
{
    IQueryable<ProductTable> ProductTables { get; }
    IQueryable<Product> Products { get; }
}
