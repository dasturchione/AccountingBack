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

    /// <summary>Boshqa lavozim/bo'limga o'tkazish. Joriy intervalni yopib, yangi ochadi (tarix saqlanadi).</summary>
    Task<Result<long>> TransferAsync(long employeeId, PayrollEmploymentTransferDto dto, CancellationToken ct = default);

    /// <summary>Oylik (oklad) o'zgartirish. Joriy intervalni yopib, yangi ochadi (tarix saqlanadi).</summary>
    Task<Result<long>> ChangePayAsync(long employeeId, PayrollEmploymentPayChangeDto dto, CancellationToken ct = default);

    /// <summary>Ishdan bo'shatish. Joriy intervalga tugash sanasini qo'yadi va xodimni passiv qiladi.</summary>
    Task<Result> DismissAsync(long employeeId, PayrollEmploymentDismissDto dto, CancellationToken ct = default);

    /// <summary>Kadr tarixi taymlayni (eskidan yangiga).</summary>
    Task<Result<List<PayrollEmploymentDto>>> GetHistoryAsync(long employeeId, CancellationToken ct = default);
    Task<Result<long>> AssignComponentAsync(long employeeId, PayrollEmployeeComponentSaveDto dto, CancellationToken ct = default);
    Task<Result> RemoveComponentAsync(long employeeId, long assignmentId, CancellationToken ct = default);
}
