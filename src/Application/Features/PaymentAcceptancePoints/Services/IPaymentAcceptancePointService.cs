using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePoints;

public interface IPaymentAcceptancePointService
{
    Task<Result<PagedResponse<PaymentAcceptancePointListDto>>> GetAllAsync(PaymentAcceptancePointListFilter filter, CancellationToken ct = default);
    Task<Result<PaymentAcceptancePointDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(PaymentAcceptancePointCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, PaymentAcceptancePointUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
