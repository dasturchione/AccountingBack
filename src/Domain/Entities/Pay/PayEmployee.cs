using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_employee")]
[Index(nameof(OrganizationId), nameof(EmployeeNumber), Name = "ux_pay_employee_org_number", IsUnique = true)]
[Index(nameof(OrganizationId), Name = "idx_pay_employee_organization_id")]
[Index(nameof(StateId), Name = "idx_pay_employee_state_id")]
public partial class PayEmployee
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("employee_number")]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = null!;

    [Column("pinfl")]
    [StringLength(14)]
    public string? Pinfl { get; set; }

    [Column("tin")]
    [StringLength(20)]
    public string? Tin { get; set; }

    [Column("first_name")]
    [StringLength(150)]
    public string FirstName { get; set; } = null!;

    [Column("last_name")]
    [StringLength(150)]
    public string LastName { get; set; } = null!;

    [Column("middle_name")]
    [StringLength(150)]
    public string? MiddleName { get; set; }

    [Column("birth_date", TypeName = "date")]
    public DateOnly? BirthDate { get; set; }

    [Column("phone_number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    [Column("email")]
    [StringLength(200)]
    public string? Email { get; set; }

    [Column("bank_account_number")]
    [StringLength(100)]
    public string? BankAccountNumber { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("created_by_user_id")]
    public int? CreatedByUserId { get; set; }

    [Column("updated_by_user_id")]
    public int? UpdatedByUserId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(CreatedByUserId))]
    public virtual User? CreatedByUser { get; set; }

    [ForeignKey(nameof(UpdatedByUserId))]
    public virtual User? UpdatedByUser { get; set; }

    public virtual ICollection<PayEmployment> Employments { get; set; } = new List<PayEmployment>();
    public virtual ICollection<PayEmployeeComponent> EmployeeComponents { get; set; } = new List<PayEmployeeComponent>();
    public virtual ICollection<PayTimesheetLine> TimesheetLines { get; set; } = new List<PayTimesheetLine>();
    public virtual ICollection<PayPayrollLine> PayrollLines { get; set; } = new List<PayPayrollLine>();
    public virtual ICollection<PayPaymentLine> PaymentLines { get; set; } = new List<PayPaymentLine>();
}
