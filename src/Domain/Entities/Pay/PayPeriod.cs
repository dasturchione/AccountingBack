using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_period")]
[Index(nameof(OrganizationId), nameof(PeriodYear), nameof(PeriodMonth), Name = "ux_pay_period_org_year_month", IsUnique = true)]
[Index(nameof(OrganizationId), Name = "idx_pay_period_organization_id")]
[Index(nameof(Status), Name = "idx_pay_period_status")]
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

    [Column("start_date", TypeName = "date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date", TypeName = "date")]
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

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(ClosedByUserId))]
    public virtual User? ClosedByUser { get; set; }

    public virtual ICollection<PayTimesheet> Timesheets { get; set; } = new List<PayTimesheet>();
    public virtual ICollection<PayPayrollDoc> PayrollDocs { get; set; } = new List<PayPayrollDoc>();
    public virtual ICollection<PayPaymentBatch> PaymentBatches { get; set; } = new List<PayPaymentBatch>();
}
