using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("acc_accounting_period")]
[Index("OrganizationId", "Year", "Month", Name = "acc_accounting_period_unique_period", IsUnique = true)]
[Index("IsClosed", Name = "idx_acc_accounting_period_is_closed")]
[Index("OrganizationId", Name = "idx_acc_accounting_period_organization_id")]
public partial class AccountingPeriod
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("year")]
    public short Year { get; set; }

    [Column("month")]
    public short Month { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("is_closed")]
    public bool IsClosed { get; set; }

    [Column("closed_at", TypeName = "timestamp without time zone")]
    public DateTime? ClosedAt { get; set; }

    [Column("closed_by_user_id")]
    public int? ClosedByUserId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }
}
