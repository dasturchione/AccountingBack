using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Payments;

public interface IPayrollPaymentService
{
    Task<Result<PagedResponse<PayrollPaymentListDto>>> GetAllAsync(PayrollPaymentListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollPaymentDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PayrollPaymentCreateDto dto, CancellationToken ct = default);
    Task<Result> ConfirmAsync(long id, CancellationToken ct = default);
    Task<Result> CancelAsync(long id, CancellationToken ct = default);
}
