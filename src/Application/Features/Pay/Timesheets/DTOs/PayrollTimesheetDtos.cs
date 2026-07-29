using SharedKernel.Filters;

namespace Application.Features.Pay.Timesheets;

public class PayrollTimesheetSaveDto
{
    public long PeriodId { get; set; }
    public DateTime DocDate { get; set; }
    public string? Note { get; set; }
    public List<PayrollTimesheetLineSaveDto> Lines { get; set; } = [];
}

public sealed class PayrollTimesheetCreateDto : PayrollTimesheetSaveDto;
public sealed class PayrollTimesheetUpdateDto : PayrollTimesheetSaveDto;

public class PayrollTimesheetLineSaveDto
{
    public long EmployeeId { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal LeaveDays { get; set; }
    public decimal SickDays { get; set; }
    public decimal AbsentDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public string? Note { get; set; }
}

public sealed class PayrollTimesheetListFilter : ISearchFilter, IPaginationFilter
{
    public string? Search { get; set; }
    public long? PeriodId { get; set; }
    public short? StatusId { get; set; }
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
}

public class PayrollTimesheetListDto
{
    public long Id { get; set; }
    public string DocNumber { get; set; } = null!;
    public DateTime DocDate { get; set; }
    public long PeriodId { get; set; }
    public string PeriodName { get; set; } = null!;
    public short StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public int EmployeeCount { get; set; }
    public string? Note { get; set; }
}

public sealed class PayrollTimesheetDto : PayrollTimesheetListDto
{
    public int OrganizationId { get; set; }
    public short StateId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<PayrollTimesheetLineDto> Lines { get; set; } = [];
}

public sealed class PayrollTimesheetLineDto : PayrollTimesheetLineSaveDto
{
    public long Id { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string EmployeeName { get; set; } = null!;
}
