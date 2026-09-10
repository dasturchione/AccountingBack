using SharedKernel.Filters;
using SharedKernel.Constants;

namespace Application.Features.Pay.Periods;

public abstract class PayrollPeriodSaveDto
{
    public short Year { get; set; }
    public short Month { get; set; }
    public decimal DailyWorkHours { get; set; }
    public List<DateOnly> WorkDates { get; set; } = [];
    public List<PayrollPeriodCalendarDaySaveDto> CalendarDays { get; set; } = [];
}

public sealed class PayrollPeriodCalendarDaySaveDto
{
    public DateOnly Date { get; set; }
    public string DayType { get; set; } = PayrollPeriodDayTypeConst.Normal;
    public decimal WorkHours { get; set; }
}

public sealed class PayrollPeriodCreateDto : PayrollPeriodSaveDto;

public sealed class PayrollPeriodUpdateDto : PayrollPeriodSaveDto;

public sealed class PayrollPeriodListFilter : IPaginationFilter
{
    public short? Year { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public sealed class PayrollPeriodDto
{
    public long Id { get; set; }
    public int OrganizationId { get; set; }
    public short Year { get; set; }
    public short Month { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DailyWorkHours { get; set; }
    public List<DateOnly> WorkDates { get; set; } = [];
    public List<PayrollPeriodCalendarDaySaveDto> CalendarDays { get; set; } = [];
    public decimal NormWorkDays { get; set; }
    public decimal NormWorkHours { get; set; }
    public string Status { get; set; } = null!;
    public bool IsUsedInTimesheet { get; set; }
    public bool CanEdit { get; set; }
    public string? EditBlockedReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}
