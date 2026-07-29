using SharedKernel.Results;

namespace Application.Features.Pay.Reports;

public interface IPayrollReportService
{
    Task<Result<PayrollRegisterReportDto>> GetRegisterAsync(long periodId, CancellationToken ct = default);
    Task<Result<PayrollPayslipDto>> GetPayslipAsync(long periodId, long employeeId, CancellationToken ct = default);
}
