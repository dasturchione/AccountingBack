using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_employment")]
[Index("StartDate", "EndDate", Name = "idx_pay_employment_dates")]
[Index("DepartmentId", Name = "idx_pay_employment_department_id")]
[Index("EmployeeId", Name = "idx_pay_employment_employee_id")]
[Index("OrganizationId", Name = "idx_pay_employment_organization_id")]
[Index("PositionId", Name = "idx_pay_employment_position_id")]
[Index("StateId", Name = "idx_pay_employment_state_id")]
[Index("EmployeeId", "StartDate", Name = "ux_pay_employment_employee_start", IsUnique = true)]
public partial class PayEmployment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("employee_id")]
    public long EmployeeId { get; set; }

    [Column("department_id")]
    public int? DepartmentId { get; set; }

    [Column("position_id")]
    public int? PositionId { get; set; }

    [Column("employment_type")]
    [StringLength(30)]
    public string EmploymentType { get; set; } = null!;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("monthly_salary")]
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }

    [Column("employment_rate")]
    [Precision(5, 4)]
    public decimal EmploymentRate { get; set; }

    [Column("weekly_hours")]
    [Precision(6, 2)]
    public decimal WeeklyHours { get; set; }

    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Column("expense_account_id")]
    public int? ExpenseAccountId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("PayEmployments")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("DepartmentId")]
    [InverseProperty("PayEmployments")]
    public virtual OrgDepartment? Department { get; set; }

    [ForeignKey("EmployeeId")]
    [InverseProperty("PayEmployments")]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey("ExpenseAccountId")]
    [InverseProperty("PayEmployments")]
    public virtual AccChartAccount? ExpenseAccount { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayEmployments")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Employment")]
    public virtual ICollection<PayPayrollLine> PayPayrollLines { get; set; } = new List<PayPayrollLine>();

    [ForeignKey("PositionId")]
    [InverseProperty("PayEmployments")]
    public virtual OrgPosition? Position { get; set; }

    [ForeignKey("StateId")]
    [InverseProperty("PayEmployments")]
    public virtual CmnState State { get; set; } = null!;
}
