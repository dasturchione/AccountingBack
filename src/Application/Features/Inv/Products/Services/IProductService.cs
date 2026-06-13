using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Products;

public interface IProductService
{
    Task<Result<PagedResponse<ProductListDto>>> GetAllAsync(ProductListFilter filter, CancellationToken ct = default);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(ProductCreateDto dto, CancellationToken ct = default);
    Task<Result> CreateManyAsync(ProductsCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
