using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Taxes;

public interface IPayrollTaxDefinitionService
{
    Task<Result<PagedResponse<PayrollTaxDefinitionListDto>>> GetAllAsync(PayrollTaxDefinitionListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollTaxDefinitionDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(PayrollTaxDefinitionCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, PayrollTaxDefinitionUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
}
