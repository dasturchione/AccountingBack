using SharedKernel.Filters;

namespace Application.Features.Pay.HrOrders;

/// <summary>
/// Kadr buyrug'i (prikaz) yaratish/tahrirlash. Target maydonlar buyruq turiga qarab
/// ishlatiladi: HIRE/TRANSFER — lavozim/bo'lim/oklad; PAY_CHANGE — oklad; DISMISSAL —
/// faqat effective_date. Ko'rsatilmagan maydonlar tasdiqда joriy intervaldan ko'chiriladi.
/// </summary>
public class PayrollHrOrderSaveDto
{
    public DateOnly OrderDate { get; set; }
    public string OrderType { get; set; } = null!;
    public long EmployeeId { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string? Basis { get; set; }
    public string? Note { get; set; }

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
}

public sealed class PayrollHrOrderListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public long? EmployeeId { get; set; }
    public string? OrderType { get; set; }
    public short? StatusId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public sealed class PayrollHrOrderListDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateOnly OrderDate { get; set; }
    public string OrderType { get; set; } = null!;
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public short StatusId { get; set; }
    public int? PositionId { get; set; }
    public string? PositionName { get; set; }
    public decimal? MonthlySalary { get; set; }
}

public sealed class PayrollHrOrderDto : PayrollHrOrderSaveDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public short StatusId { get; set; }
    public string EmployeeName { get; set; } = null!;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? CurrencyName { get; set; }
    public string? ExpenseAccountNumber { get; set; }
    public long? EmploymentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
}

/// <summary>Bosma buyruq: eski (joriy/predecessor) va yangi qiymatlar bilan.</summary>
public sealed class PayrollHrOrderPrintDto
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public DateOnly OrderDate { get; set; }
    public string OrderType { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public short StatusId { get; set; }
    public string? Basis { get; set; }
    public string? Note { get; set; }

    public string OrganizationName { get; set; } = null!;
    public long EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
    public string? Pinfl { get; set; }

    // Eski (o'zgarishdan oldingi) holat.
    public string? FromDepartmentName { get; set; }
    public string? FromPositionName { get; set; }
    public decimal? FromMonthlySalary { get; set; }

    // Yangi (buyruq bo'yicha) holat.
    public string? ToDepartmentName { get; set; }
    public string? ToPositionName { get; set; }
    public decimal? ToMonthlySalary { get; set; }
    public string? CurrencyName { get; set; }
    public decimal? EmploymentRate { get; set; }

    public string? ConfirmedByName { get; set; }
    public DateTime? ConfirmedDate { get; set; }
}
