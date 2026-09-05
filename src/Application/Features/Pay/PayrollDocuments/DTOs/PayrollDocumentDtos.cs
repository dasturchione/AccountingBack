using SharedKernel.Filters;

namespace Application.Features.Pay.PayrollDocuments;

public sealed class PayrollCalculateDto
{
    public long PeriodId { get; set; }
    public DateTime DocDate { get; set; }
    public string DocumentKind { get; set; } = "REGULAR";
    public long? CorrectionOfDocId { get; set; }
    public int SalaryExpenseAccountId { get; set; }
    public int SalaryPayableAccountId { get; set; }
    public int DeductionPayableAccountId { get; set; }
    public int EmployerTaxExpenseAccountId { get; set; }
    public int EmployerTaxPayableAccountId { get; set; }
    public int AdvanceReceivableAccountId { get; set; }
    public string? Note { get; set; }
    public List<PayrollManualAdjustmentDto> Adjustments { get; set; } = [];
}

public sealed class PayrollManualAdjustmentDto
{
    public long EmployeeId { get; set; }
    public int ComponentId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

public sealed class PayrollDocumentListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public long? PeriodId { get; set; }
    public short? StatusId { get; set; }
    public string? DocumentKind { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class PayrollDocumentListDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public string DocumentKind { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal EmployerTaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PayableAmount { get; set; }
}

public sealed class PayrollDocumentDto : PayrollDocumentListDto
{
    public int OrganizationId { get; set; }
    public long? CorrectionOfDocId { get; set; }
    public short CurrencyId { get; set; }
    public int? SalaryExpenseAccountId { get; set; }
    public int? SalaryPayableAccountId { get; set; }
    public int? DeductionPayableAccountId { get; set; }
    public int? EmployerTaxExpenseAccountId { get; set; }
    public int? EmployerTaxPayableAccountId { get; set; }
    public int? AdvanceReceivableAccountId { get; set; }
    public decimal AdvanceAmount { get; set; }
    public string? Note { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<PayrollLineDto> Lines { get; set; } = [];
}

public sealed class PayrollLineDto
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public long EmploymentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal EmployerTaxAmount { get; set; }
    public decimal AdvanceAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public List<PayrollCalcLineDto> CalcLines { get; set; } = [];
}

public sealed class PayrollCalcLineDto
{
    public long Id { get; set; }
    public int ComponentId { get; set; }
    public string ComponentCode { get; set; } = null!;
    public string ComponentName { get; set; } = null!;
    public string ComponentType { get; set; } = null!;
    public string CalculationMethod { get; set; } = null!;
    public decimal BaseAmount { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Rate { get; set; }
    public decimal Amount { get; set; }
    public int? DebitAccountId { get; set; }
    public int? CreditAccountId { get; set; }
    public bool IsManual { get; set; }
    public string? Note { get; set; }
}
