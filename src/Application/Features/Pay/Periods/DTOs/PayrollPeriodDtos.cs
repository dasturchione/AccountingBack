using SharedKernel.Filters;

namespace Application.Features.Pay.Periods;

public sealed class PayrollPeriodCreateDto
{
    public short Year { get; set; }
    public short Month { get; set; }
    public decimal NormWorkDays { get; set; }
    public decimal NormWorkHours { get; set; }
}

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
    public decimal NormWorkDays { get; set; }
    public decimal NormWorkHours { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}
