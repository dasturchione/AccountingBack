using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

/// <summary>
/// Effective-dated inputs copied into a payroll line.  A posted payroll must
/// remain reproducible even when an employment or its component assignment is
/// subsequently changed.
/// </summary>
[Table("pay_payroll_line_segment")]
[Index(nameof(OrganizationId), Name = "idx_pay_payroll_line_segment_organization_id")]
[Index(nameof(PayrollLineId), nameof(SegmentStartDate), Name = "ux_pay_payroll_line_segment_line_start", IsUnique = true)]
[Index(nameof(EmploymentId), nameof(SegmentStartDate), Name = "idx_pay_payroll_line_segment_employment_start")]
public partial class PayPayrollLineSegment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payroll_line_id")]
    public long PayrollLineId { get; set; }

    [Column("employment_id")]
    public long EmploymentId { get; set; }

    [Column("segment_start_date", TypeName = "date")]
    public DateOnly SegmentStartDate { get; set; }

    [Column("segment_end_date", TypeName = "date")]
    public DateOnly SegmentEndDate { get; set; }

    [Column("monthly_salary")]
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }

    [Column("employment_rate")]
    [Precision(5, 4)]
    public decimal EmploymentRate { get; set; }

    [Column("worked_days")]
    [Precision(6, 2)]
    public decimal WorkedDays { get; set; }

    [Column("worked_hours")]
    [Precision(8, 2)]
    public decimal WorkedHours { get; set; }

    [Column("norm_work_days")]
    [Precision(6, 2)]
    public decimal NormWorkDays { get; set; }

    [Column("norm_work_hours")]
    [Precision(8, 2)]
    public decimal NormWorkHours { get; set; }

    /// <summary>
    /// JSON array of effective employee-component assignments for this date
    /// range.  Keeping the assignment values with the line prevents a later
    /// component edit from changing a posted document's source inputs.
    /// </summary>
    [Column("component_snapshot_json", TypeName = "jsonb")]
    public string ComponentSnapshotJson { get; set; } = "[]";

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PayrollLineId))]
    public virtual PayPayrollLine PayrollLine { get; set; } = null!;

    [ForeignKey(nameof(EmploymentId))]
    public virtual PayEmployment Employment { get; set; } = null!;
}
