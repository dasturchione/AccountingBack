using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_employment")]
[Index(nameof(OrganizationId), Name = "idx_pay_employment_organization_id")]
[Index(nameof(EmployeeId), Name = "idx_pay_employment_employee_id")]
[Index(nameof(StartDate), nameof(EndDate), Name = "idx_pay_employment_dates")]
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

    [Column("start_date", TypeName = "date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
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

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual PayEmployee Employee { get; set; } = null!;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }

    [ForeignKey(nameof(PositionId))]
    public virtual Position? Position { get; set; }

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey(nameof(ExpenseAccountId))]
    public virtual ChartAccount? ExpenseAccount { get; set; }

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    public virtual ICollection<PayPayrollLine> PayrollLines { get; set; } = new List<PayPayrollLine>();
}
