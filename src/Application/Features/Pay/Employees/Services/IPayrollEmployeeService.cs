using Application.Common.Pagination;
using SharedKernel.Results;

namespace Application.Features.Pay.Employees;

public interface IPayrollEmployeeService
{
    Task<Result<PagedResponse<PayrollEmployeeListDto>>> GetAllAsync(PayrollEmployeeListFilter filter, CancellationToken ct = default);
    Task<Result<PayrollEmployeeDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Result<long>> CreateAsync(PayrollEmployeeCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(long id, PayrollEmployeeUpdateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
    Task<Result<long>> AddEmploymentAsync(long employeeId, PayrollEmploymentSaveDto dto, CancellationToken ct = default);
    Task<Result> UpdateEmploymentAsync(long employeeId, long employmentId, PayrollEmploymentSaveDto dto, CancellationToken ct = default);
    Task<Result<long>> AssignComponentAsync(long employeeId, PayrollEmployeeComponentSaveDto dto, CancellationToken ct = default);
    Task<Result> RemoveComponentAsync(long employeeId, long assignmentId, CancellationToken ct = default);
}
