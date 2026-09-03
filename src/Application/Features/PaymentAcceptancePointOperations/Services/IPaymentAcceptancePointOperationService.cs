using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.PaymentAcceptancePointOperations;

public interface IPaymentAcceptancePointOperationService
{
    Task<Result<PagedResponse<PaymentAcceptancePointOperationListDto>>> GetAllAsync(
        PaymentAcceptancePointOperationListFilter filter,
        CancellationToken ct = default);

    Task<Result<PaymentAcceptancePointOperationDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PaymentAcceptancePointOperationCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PaymentAcceptancePointOperationUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
    Task<Result<PaymentAcceptancePointBalanceDto>> GetBalanceAsync(
        int paymentAcceptancePointId,
        short currencyId,
        DateTime? asOfDate,
        CancellationToken ct = default);
}
