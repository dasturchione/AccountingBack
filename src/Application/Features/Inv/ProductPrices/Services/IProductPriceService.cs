using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Inv.ProductPrices;

public interface IProductPriceService
{
    Task<Result<PagedResponse<ProductPriceListDto>>> GetAllAsync(ProductPriceListFilter filter, CancellationToken ct = default);
    Task<Result<ProductPriceDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<ProductPriceDetailsDto>> GetPriceDetailsByProductIdAsync(int productId, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(ProductPriceCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, ProductPriceUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
