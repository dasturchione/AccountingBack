using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_timesheet_line_day")]
[Index(nameof(TimesheetLineId), nameof(WorkDate), Name = "ux_pay_timesheet_line_day_line_date", IsUnique = true)]
[Index(nameof(OrganizationId), nameof(WorkDate), Name = "idx_pay_timesheet_line_day_organization_date")]
public partial class PayTimesheetLineDay
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("timesheet_line_id")]
    public long TimesheetLineId { get; set; }

    [Column("work_date", TypeName = "date")]
    public DateOnly WorkDate { get; set; }

    [Column("source_status_code")]
    [StringLength(50)]
    public string SourceStatusCode { get; set; } = null!;

    [Column("source_absence_id")]
    public long? SourceAbsenceId { get; set; }

    [Column("source_schedule_id")]
    public long? SourceScheduleId { get; set; }

    [Column("source_absence_type_id")]
    public short? SourceAbsenceTypeId { get; set; }

    [Column("status_code")]
    [StringLength(50)]
    public string StatusCode { get; set; } = null!;

    [Column("absence_type_id")]
    public short? AbsenceTypeId { get; set; }

    [Column("timesheet_category")]
    [StringLength(20)]
    public string? TimesheetCategory { get; set; }

    [Column("worked_hours")]
    [Precision(8, 2)]
    public decimal WorkedHours { get; set; }

    [Column("planned_hours")]
    [Precision(8, 2)]
    public decimal PlannedHours { get; set; }

    [Column("overtime_hours")]
    [Precision(8, 2)]
    public decimal OvertimeHours { get; set; }

    [Column("night_hours")]
    [Precision(8, 2)]
    public decimal NightHours { get; set; }

    [Column("holiday_hours")]
    [Precision(8, 2)]
    public decimal HolidayHours { get; set; }

    [Column("weekend_hours")]
    [Precision(8, 2)]
    public decimal WeekendHours { get; set; }

    [ForeignKey(nameof(TimesheetLineId))]
    public virtual PayTimesheetLine TimesheetLine { get; set; } = null!;
}
