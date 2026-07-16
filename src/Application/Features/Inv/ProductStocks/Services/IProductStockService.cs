using Application.Common.Pagination;
using Application.Features.Inv.WarehouseProducts;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public interface IProductStockService
{
    Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default);
    Task<Result<PagedResponse<ProductGroupStockDto>>> GetProductGroupsStockAsync(ProductGroupStockFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<WarehouseProductDto>>> GetProductsStockAsync(ProductStockFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<ProductTableStockDto>>> GetProductTablesStockAsync(ProductTableStockFilter filter, CancellationToken ct = default);
}
