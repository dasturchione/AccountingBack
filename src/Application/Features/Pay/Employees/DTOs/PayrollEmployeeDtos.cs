using SharedKernel.Constants;
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

    /// <summary>Avans usuli: PERCENT/FIXED (default PERCENT).</summary>
    public string AdvanceMethod { get; set; } = PayrollAdvanceMethodConst.Percent;

    /// <summary>PERCENT uchun foiz (0..100), FIXED uchun summa. Default 0 (avans yo'q).</summary>
    public decimal AdvanceValue { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Kadr o'tkazish (transfer): boshqa lavozim/bo'limga o'tkazish. Ko'rsatilmagan
/// maydonlar (oklad, stavka, valyuta, avans ...) joriy intervaldan avtomatik ko'chiriladi.
/// </summary>
public sealed class PayrollEmploymentTransferDto
{
    public DateOnly EffectiveDate { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? MonthlySalary { get; set; }
    public decimal? EmploymentRate { get; set; }
    public decimal? WeeklyHours { get; set; }
    public short? CurrencyId { get; set; }
    public int? ExpenseAccountId { get; set; }
    public string? AdvanceMethod { get; set; }
    public decimal? AdvanceValue { get; set; }
    public string? Note { get; set; }
}

/// <summary>Oylik (oklad) o'zgartirish: lavozim/bo'lim joriy intervaldan ko'chiriladi.</summary>
public sealed class PayrollEmploymentPayChangeDto
{
    public DateOnly EffectiveDate { get; set; }
    public decimal MonthlySalary { get; set; }
    public decimal? EmploymentRate { get; set; }
    public string? Note { get; set; }
}

/// <summary>Ishdan bo'shatish: joriy interval yopiladi va xodim passiv holatga o'tadi.</summary>
public sealed class PayrollEmploymentDismissDto
{
    public DateOnly EffectiveDate { get; set; }
    public string? Note { get; set; }
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
    public string ActionType { get; set; } = null!;
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
