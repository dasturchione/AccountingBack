using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Payments;

public interface IPayrollPaymentService
{
    Task<Result<PagedResponse<PayrollPaymentListDto>>> GetAllAsync(PayrollPaymentListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollPaymentDto>> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Avans qaydnomasini prefill uchun: xodim bo'yicha hisoblangan avans + WITH_ADVANCE tuzatishlar.</summary>
    Task<Result<PayrollAdvanceSuggestionDto>> GetAdvanceSuggestionAsync(long periodId, CancellationToken ct = default);

    Task<Result<long>> CreateAsync(PayrollPaymentCreateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
