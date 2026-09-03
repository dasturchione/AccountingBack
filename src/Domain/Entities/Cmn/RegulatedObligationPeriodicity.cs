using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_regulated_obligation_periodicity")]
public sealed class RegulatedObligationPeriodicity
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(Entities.State.RegulatedObligationPeriodicities))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(RegulatedObligationPeriodicityTranslation.Periodicity))]
    public ICollection<RegulatedObligationPeriodicityTranslation> Translations { get; set; } = [];

    [InverseProperty(nameof(OrganizationRegulatedObligationSetting.Periodicity))]
    public ICollection<OrganizationRegulatedObligationSetting> OrganizationSettings { get; set; } = [];
}
