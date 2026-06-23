using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductStocks;

public interface IProductStockService
{
    Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default);
    Task<Result<PagedResponse<ProductGroupStockDto>>> GetProductGroupsStockAsync(ProductGroupStockFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<ProductStockDto>>> GetProductsStockAsync(ProductStockFilter filter, CancellationToken ct = default);
    Task<Result<PagedResponse<ProductTableStockDto>>> GetProductTablesStockAsync(ProductTableStockFilter filter, CancellationToken ct = default);
}
