using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_timesheet_line")]
[Index("EmployeeId", Name = "idx_pay_timesheet_line_employee_id")]
[Index("OrganizationId", Name = "idx_pay_timesheet_line_organization_id")]
[Index("TimesheetId", Name = "idx_pay_timesheet_line_timesheet_id")]
[Index("TimesheetId", "EmployeeId", Name = "ux_pay_timesheet_line_employee", IsUnique = true)]
public partial class PayTimesheetLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("timesheet_id")]
    public long TimesheetId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("worked_days")]
    [Precision(6, 2)]
    public decimal WorkedDays { get; set; }

    [Column("worked_hours")]
    [Precision(8, 2)]
    public decimal WorkedHours { get; set; }

    [Column("leave_days")]
    [Precision(6, 2)]
    public decimal LeaveDays { get; set; }

    [Column("sick_days")]
    [Precision(6, 2)]
    public decimal SickDays { get; set; }

    [Column("absent_days")]
    [Precision(6, 2)]
    public decimal AbsentDays { get; set; }

    [Column("overtime_hours")]
    [Precision(8, 2)]
    public decimal OvertimeHours { get; set; }

    [Column("note")]
    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("PayTimesheetLines")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayTimesheetLines")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [ForeignKey("TimesheetId")]
    [InverseProperty("PayTimesheetLines")]
    public virtual PayTimesheet Timesheet { get; set; } = null!;
}
