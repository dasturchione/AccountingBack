using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_payroll_line")]
[Index("EmployeeId", Name = "idx_pay_payroll_line_employee_id")]
[Index("EmploymentId", Name = "idx_pay_payroll_line_employment_id")]
[Index("OrganizationId", Name = "idx_pay_payroll_line_organization_id")]
[Index("PayrollDocId", Name = "idx_pay_payroll_line_payroll_doc_id")]
[Index("PayrollDocId", "EmployeeId", Name = "ux_pay_payroll_line_employee", IsUnique = true)]
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

    [ForeignKey("EmployeeId")]
    [InverseProperty("PayPayrollLines")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey("EmploymentId")]
    [InverseProperty("PayPayrollLines")]
    public virtual PayEmployment Employment { get; set; } = null!;

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPayrollLines")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("PayrollLine")]
    public virtual ICollection<PayPaymentLine> PayPaymentLines { get; set; } = new List<PayPaymentLine>();

    [InverseProperty("PayrollLine")]
    public virtual ICollection<PayPayrollCalcLine> PayPayrollCalcLines { get; set; } = new List<PayPayrollCalcLine>();

    [ForeignKey("PayrollDocId")]
    [InverseProperty("PayPayrollLines")]
    public virtual PayPayrollDoc PayrollDoc { get; set; } = null!;
}
