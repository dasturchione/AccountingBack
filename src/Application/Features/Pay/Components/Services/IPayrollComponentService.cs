using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Components;

public interface IPayrollComponentService
{
    Task<Result<PagedResponse<PayrollComponentListDto>>> GetAllAsync(PayrollComponentListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollComponentDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(PayrollComponentCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, PayrollComponentUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
