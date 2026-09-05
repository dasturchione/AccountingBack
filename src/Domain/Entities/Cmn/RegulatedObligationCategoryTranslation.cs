using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(CategoryId), nameof(LanguageId))]
[Table("cmn_regulated_obligation_category_translation")]
public sealed class RegulatedObligationCategoryTranslation
{
    [Key]
    [Column("category_id")]
    public short CategoryId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    [InverseProperty(nameof(RegulatedObligationCategory.Translations))]
    public RegulatedObligationCategory Category { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.RegulatedObligationCategoryTranslations))]
    public Language Language { get; set; } = null!;
}
