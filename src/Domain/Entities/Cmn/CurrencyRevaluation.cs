using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_currency_revaluation")]
[Index("OrganizationId", Name = "idx_cmn_currency_revaluation_organization_id")]
[Index("RevaluationDate", Name = "idx_cmn_currency_revaluation_revaluation_date")]
[Index("PostedByUserId", Name = "idx_cmn_currency_revaluation_posted_by_user_id")]
[Index("CancelledByUserId", Name = "idx_cmn_currency_revaluation_cancelled_by_user_id")]
[Index("StateId", Name = "idx_cmn_currency_revaluation_state_id")]
[Index("StatusId", Name = "idx_cmn_currency_revaluation_status_id")]
public partial class CurrencyRevaluation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("revaluation_date", TypeName = "timestamp without time zone")]
    public DateTime RevaluationDate { get; set; }

    [Column("provider_rate_date", TypeName = "timestamp without time zone")]
    public DateTime? ProviderRateDate { get; set; }

    [Column("status_id")]
    public short StatusId { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("confirmed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("posted_by_user_id")]
    public int? PostedByUserId { get; set; }

    [Column("cancelled_at", TypeName = "timestamp without time zone")]
    public DateTime? CancelledAt { get; set; }

    [Column("cancelled_by_user_id")]
    public int? CancelledByUserId { get; set; }

    [InverseProperty(nameof(CurrencyRevaluationLine.Revaluation))]
    public virtual ICollection<CurrencyRevaluationLine> Lines { get; set; } = new List<CurrencyRevaluationLine>();

    [ForeignKey(nameof(StateId))]
    [InverseProperty("CurrencyRevaluations")]
    public virtual State State { get; set; } = null!;

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty("CurrencyRevaluations")]
    public virtual Organization Organization { get; set; } = null!;
}
