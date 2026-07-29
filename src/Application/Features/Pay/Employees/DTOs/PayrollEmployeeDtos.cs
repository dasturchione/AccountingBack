using SharedKernel.Filters;

namespace Application.Features.Pay.Employees;

public class PayrollEmployeeBaseDto
{
    public string EmployeeNumber { get; set; } = null!;
    public string? Pinfl { get; set; }
    public string? Tin { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? BankAccountNumber { get; set; }
}

public sealed class PayrollEmployeeCreateDto : PayrollEmployeeBaseDto
{
    public PayrollEmploymentSaveDto Employment { get; set; } = new();
}

public sealed class PayrollEmployeeUpdateDto : PayrollEmployeeBaseDto;

public class PayrollEmploymentSaveDto
{
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public string EmploymentType { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal MonthlySalary { get; set; }
    public decimal EmploymentRate { get; set; } = 1m;
    public decimal WeeklyHours { get; set; } = 40m;
    public short CurrencyId { get; set; }
    public int? ExpenseAccountId { get; set; }
}

public class PayrollEmployeeComponentSaveDto
{
    public int ComponentId { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Rate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}

public sealed class PayrollEmployeeListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public short? StateId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public sealed class PayrollEmployeeListDto
{
    public long Id { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Pinfl { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? PositionId { get; set; }
    public string? PositionName { get; set; }
    public decimal? MonthlySalary { get; set; }
    public short StateId { get; set; }
}

public sealed class PayrollEmployeeDto : PayrollEmployeeBaseDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<PayrollEmploymentDto> Employments { get; set; } = [];
    public List<PayrollEmployeeComponentDto> Components { get; set; } = [];
}

public sealed class PayrollEmploymentDto : PayrollEmploymentSaveDto
{
    public long Id { get; set; }
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string CurrencyName { get; set; } = null!;
    public string? ExpenseAccountNumber { get; set; }
    public short StateId { get; set; }
}

public sealed class PayrollEmployeeComponentDto : PayrollEmployeeComponentSaveDto
{
    public long Id { get; set; }
    public string ComponentCode { get; set; } = null!;
    public string ComponentName { get; set; } = null!;
    public string ComponentType { get; set; } = null!;
    public short StateId { get; set; }
}
