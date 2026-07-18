using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.SaleShipments;

public interface ISaleShipmentService
{
    Task<Result<long>> CreateAsync(SaleShipmentCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, SaleShipmentUpdateDto dto, CancellationToken ct = default);
    Task<Result<SaleShipmentDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<PagedResponse<SaleShipmentListDto>>> GetListAsync(SaleShipmentFilter filter, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
