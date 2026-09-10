using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_payroll_line")]
[Index(nameof(OrganizationId), Name = "idx_pay_payroll_line_organization_id")]
[Index(nameof(PayrollDocId), Name = "idx_pay_payroll_line_payroll_doc_id")]
[Index(nameof(EmployeeId), Name = "idx_pay_payroll_line_employee_id")]
public partial class PayPayrollLine
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("payroll_doc_id")]
    public long PayrollDocId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("employment_id")]
    public long EmploymentId { get; set; }

    [Column("worked_days")]
    [Precision(6, 2)]
    public decimal WorkedDays { get; set; }

    [Column("worked_hours")]
    [Precision(8, 2)]
    public decimal WorkedHours { get; set; }

    [Column("paid_leave_days")]
    [Precision(6, 2)]
    public decimal PaidLeaveDays { get; set; }

    [Column("paid_sick_days")]
    [Precision(6, 2)]
    public decimal PaidSickDays { get; set; }

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

    [Column("gross_amount")]
    [Precision(18, 2)]
    public decimal GrossAmount { get; set; }

    [Column("deduction_amount")]
    [Precision(18, 2)]
    public decimal DeductionAmount { get; set; }

    [Column("employer_tax_amount")]
    [Precision(18, 2)]
    public decimal EmployerTaxAmount { get; set; }

    [Column("advance_amount")]
    [Precision(18, 2)]
    public decimal AdvanceAmount { get; set; }

    [Column("net_amount")]
    [Precision(18, 2)]
    public decimal NetAmount { get; set; }

    [Column("payable_amount")]
    [Precision(18, 2)]
    public decimal PayableAmount { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(PayrollDocId))]
    public virtual PayPayrollDoc PayrollDoc { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(EmploymentId))]
    public virtual PayEmployment Employment { get; set; } = null!;

    public virtual ICollection<PayPayrollCalcLine> CalcLines { get; set; } = new List<PayPayrollCalcLine>();
    public virtual ICollection<PayPayrollTaxLine> TaxLines { get; set; } = new List<PayPayrollTaxLine>();
    public virtual ICollection<PayPayrollLineSegment> Segments { get; set; } = new List<PayPayrollLineSegment>();
    public virtual ICollection<PayPaymentLine> PaymentLines { get; set; } = new List<PayPaymentLine>();
}
