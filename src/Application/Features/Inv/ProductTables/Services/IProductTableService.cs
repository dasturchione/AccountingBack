using SharedKernel.Results;

namespace Application.Features.ProductTables;

public interface IProductTableService
{
    Task<Result<ProductTableByMarkingDto>> GetByMarkingNumberAsync(string markingNumber, CancellationToken ct = default);
}
