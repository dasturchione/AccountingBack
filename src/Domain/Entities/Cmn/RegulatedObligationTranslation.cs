using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(RegulatedObligationId), nameof(LanguageId))]
[Table("cmn_regulated_obligation_translation")]
public sealed class RegulatedObligationTranslation
{
    [Key]
    [Column("regulated_obligation_id")]
    public short RegulatedObligationId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(RegulatedObligationId))]
    [InverseProperty(nameof(Entities.RegulatedObligation.Translations))]
    public RegulatedObligation RegulatedObligation { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.RegulatedObligationTranslations))]
    public Language Language { get; set; } = null!;
}
