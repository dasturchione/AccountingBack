namespace Application.Features.Pay.Reports;

public sealed class PayrollRegisterReportDto
{
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal EmployerTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public List<PayrollRegisterEmployeeDto> Employees { get; set; } = [];
}

public class PayrollRegisterEmployeeDto
{
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal EmployerTaxAmount { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public sealed class PayrollPayslipDto : PayrollRegisterEmployeeDto
{
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public List<PayrollPayslipComponentDto> Components { get; set; } = [];
}

public sealed class PayrollPayslipComponentDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string ComponentType { get; set; } = null!;
    public decimal BaseAmount { get; set; }
    public decimal? Rate { get; set; }
    public decimal Amount { get; set; }
}
