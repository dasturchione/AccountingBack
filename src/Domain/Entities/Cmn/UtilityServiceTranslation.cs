using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(UtilityServiceId), nameof(LanguageId))]
[Table("cmn_utility_service_translation")]
public sealed class UtilityServiceTranslation
{
    [Key]
    [Column("utility_service_id")]
    public short UtilityServiceId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(UtilityServiceId))]
    [InverseProperty(nameof(Entities.UtilityService.Translations))]
    public UtilityService UtilityService { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.UtilityServiceTranslations))]
    public Language Language { get; set; } = null!;
}
