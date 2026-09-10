using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("pay_period_work_day")]
[Index(nameof(PeriodId), nameof(WorkDate), Name = "ux_pay_period_work_day_period_date", IsUnique = true)]
[Index(nameof(OrganizationId), nameof(WorkDate), Name = "idx_pay_period_work_day_organization_date")]
public partial class PayPeriodWorkDay
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("period_id")]
    public long PeriodId { get; set; }

    [Column("work_date", TypeName = "date")]
    public DateOnly WorkDate { get; set; }

    [Column("day_type")]
    [StringLength(20)]
    public string DayType { get; set; } = "NORMAL";

    [Column("is_work_day")]
    public bool IsWorkDay { get; set; } = true;

    [Column("work_hours")]
    [Precision(8, 4)]
    public decimal WorkHours { get; set; }

    [ForeignKey(nameof(PeriodId))]
    public virtual PayPeriod Period { get; set; } = null!;
}
