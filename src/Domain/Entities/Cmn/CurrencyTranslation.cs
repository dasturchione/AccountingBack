using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("CurrencyId", "LanguageId")]
[Table("cmn_currency_translation")]
public partial class CurrencyTranslation
{
    [Key]
    [Column("currency_id")]
    public short CurrencyId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("CurrencyId")]
    [InverseProperty(nameof(Entities.Currency.CurrencyTranslations))]
    public virtual Currency Currency { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Entities.Language.CurrencyTranslations))]
    public virtual Language Language { get; set; } = null!;
}
