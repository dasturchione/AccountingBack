using SharedKernel.Results;

namespace Application.Features.ProductTables;

public interface IProductTableService
{
    Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default);
    Task<Result<List<ProductTableGroupSummaryDto>>> GetProductGroupSummaryAsync(CancellationToken ct = default);
    Task<Result<List<ProductTableProductSummaryDto>>> GetProductSummaryAsync(ProductTableGroupFilter filter, CancellationToken ct = default);
    Task<Result<List<ProductTableItemDto>>> GetProductTableSummaryAsync(int? productGroupId, int? productId, CancellationToken ct = default);
}
