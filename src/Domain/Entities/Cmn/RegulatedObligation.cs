using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_regulated_obligation")]
public sealed class RegulatedObligation
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("category_id")]
    public short CategoryId { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey(nameof(CategoryId))]
    [InverseProperty(nameof(RegulatedObligationCategory.RegulatedObligations))]
    public RegulatedObligationCategory Category { get; set; } = null!;

    [ForeignKey(nameof(StateId))]
    [InverseProperty(nameof(Entities.State.RegulatedObligations))]
    public State State { get; set; } = null!;

    [InverseProperty(nameof(RegulatedObligationTranslation.RegulatedObligation))]
    public ICollection<RegulatedObligationTranslation> Translations { get; set; } = [];

    [InverseProperty(nameof(OrganizationRegulatedObligationSetting.RegulatedObligation))]
    public ICollection<OrganizationRegulatedObligationSetting> OrganizationSettings { get; set; } = [];
}
