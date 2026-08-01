using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_employee")]
[Index("LastName", "FirstName", Name = "idx_pay_employee_name")]
[Index("OrganizationId", Name = "idx_pay_employee_organization_id")]
[Index("StateId", Name = "idx_pay_employee_state_id")]
[Index("OrganizationId", "EmployeeNumber", Name = "ux_pay_employee_org_number", IsUnique = true)]
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

    [Column("birth_date")]
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

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("PayEmployeeCreatedByUsers")]
    public virtual SysUser? CreatedByUser { get; set; }

    [InverseProperty("Employee")]
    public virtual ICollection<HrAbsence> HrAbsences { get; set; } = new List<HrAbsence>();

    [InverseProperty("Employee")]
    public virtual ICollection<HrEmployeeWorkSchedule> HrEmployeeWorkSchedules { get; set; } = new List<HrEmployeeWorkSchedule>();

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayEmployees")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Employee")]
    public virtual ICollection<PayEmployeeComponent> PayEmployeeComponents { get; set; } = new List<PayEmployeeComponent>();

    [InverseProperty("Employee")]
    public virtual ICollection<PayEmployment> PayEmployments { get; set; } = new List<PayEmployment>();

    [InverseProperty("Employee")]
    public virtual ICollection<PayPaymentLine> PayPaymentLines { get; set; } = new List<PayPaymentLine>();

    [InverseProperty("Employee")]
    public virtual ICollection<PayPayrollLine> PayPayrollLines { get; set; } = new List<PayPayrollLine>();

    [InverseProperty("Employee")]
    public virtual ICollection<PayTimesheetLine> PayTimesheetLines { get; set; } = new List<PayTimesheetLine>();

    [ForeignKey("StateId")]
    [InverseProperty("PayEmployees")]
    public virtual CmnState State { get; set; } = null!;

    [ForeignKey("UpdatedByUserId")]
    [InverseProperty("PayEmployeeUpdatedByUsers")]
    public virtual SysUser? UpdatedByUser { get; set; }
}
