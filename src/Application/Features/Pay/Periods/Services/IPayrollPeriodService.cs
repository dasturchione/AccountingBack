using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Periods;

public interface IPayrollPeriodService
{
    Task<Result<PagedResponse<PayrollPeriodDto>>> GetAllAsync(PayrollPeriodListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollPeriodDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PayrollPeriodCreateDto dto, CancellationToken ct = default);
    Task<Result> CloseAsync(long id, CancellationToken ct = default);
    Task<Result> ReopenAsync(long id, CancellationToken ct = default);
}
