using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("CurrencyId", "LanguageId")]
[Table("cmn_currency_translation")]
[Index("LanguageId", Name = "ix_cmn_currency_translation_language_id")]
public partial class CmnCurrencyTranslation
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
    [InverseProperty("CmnCurrencyTranslations")]
    public virtual CmnCurrency Currency { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnCurrencyTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
