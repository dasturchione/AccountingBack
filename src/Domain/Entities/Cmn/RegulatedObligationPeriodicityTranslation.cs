using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(PeriodicityId), nameof(LanguageId))]
[Table("cmn_regulated_obligation_periodicity_translation")]
public sealed class RegulatedObligationPeriodicityTranslation
{
    [Key]
    [Column("periodicity_id")]
    public short PeriodicityId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(PeriodicityId))]
    [InverseProperty(nameof(RegulatedObligationPeriodicity.Translations))]
    public RegulatedObligationPeriodicity Periodicity { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.RegulatedObligationPeriodicityTranslations))]
    public Language Language { get; set; } = null!;
}
