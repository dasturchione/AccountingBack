using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("org_regulated_obligation_setting")]
public sealed class OrganizationRegulatedObligationSetting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public int OrganizationId { get; set; }

    [Column("regulated_obligation_id")]
    public short RegulatedObligationId { get; set; }

    [Column("periodicity_id")]
    public short PeriodicityId { get; set; }

    [Column("classifier_code")]
    [StringLength(30)]
    public string? ClassifierCode { get; set; }

    [Column("rate", TypeName = "numeric(9,6)")]
    public decimal? Rate { get; set; }

    [Column("chart_account_id")]
    public int ChartAccountId { get; set; }

    [Column("effective_from")]
    public DateOnly EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateOnly? EffectiveTo { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    [InverseProperty(nameof(Entities.Organization.RegulatedObligationSettings))]
    public Organization Organization { get; set; } = null!;

    [ForeignKey(nameof(RegulatedObligationId))]
    [InverseProperty(nameof(Entities.RegulatedObligation.OrganizationSettings))]
    public RegulatedObligation RegulatedObligation { get; set; } = null!;

    [ForeignKey(nameof(PeriodicityId))]
    [InverseProperty(nameof(RegulatedObligationPeriodicity.OrganizationSettings))]
    public RegulatedObligationPeriodicity Periodicity { get; set; } = null!;

    [ForeignKey(nameof(ChartAccountId))]
    [InverseProperty(nameof(Entities.ChartAccount.RegulatedObligationSettings))]
    public ChartAccount ChartAccount { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(Entities.State.OrganizationRegulatedObligationSettings))]
    public State State { get; set; } = null!;
}
