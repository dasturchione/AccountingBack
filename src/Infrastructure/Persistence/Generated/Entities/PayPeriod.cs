using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[Table("pay_period")]
[Index("StartDate", "EndDate", Name = "idx_pay_period_dates")]
[Index("OrganizationId", Name = "idx_pay_period_organization_id")]
[Index("Status", Name = "idx_pay_period_status")]
[Index("OrganizationId", "PeriodYear", "PeriodMonth", Name = "ux_pay_period_org_year_month", IsUnique = true)]
public partial class PayPeriod
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("period_year")]
    public short PeriodYear { get; set; }

    [Column("period_month")]
    public short PeriodMonth { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("norm_work_days")]
    [Precision(6, 2)]
    public decimal NormWorkDays { get; set; }

    [Column("norm_work_hours")]
    [Precision(8, 2)]
    public decimal NormWorkHours { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("closed_date", TypeName = "timestamp without time zone")]
    public DateTime? ClosedDate { get; set; }

    [Column("closed_by_user_id")]
    public int? ClosedByUserId { get; set; }

    [ForeignKey("ClosedByUserId")]
    [InverseProperty("PayPeriods")]
    public virtual SysUser? ClosedByUser { get; set; }

    [ForeignKey("OrganizationId")]
    [InverseProperty("PayPeriods")]
    public virtual OrgOrganization Organization { get; set; } = null!;

    [InverseProperty("Period")]
    public virtual ICollection<PayPaymentBatch> PayPaymentBatches { get; set; } = new List<PayPaymentBatch>();

    [InverseProperty("Period")]
    public virtual ICollection<PayPayrollDoc> PayPayrollDocs { get; set; } = new List<PayPayrollDoc>();

    [InverseProperty("Period")]
    public virtual ICollection<PayTimesheet> PayTimesheets { get; set; } = new List<PayTimesheet>();
}
