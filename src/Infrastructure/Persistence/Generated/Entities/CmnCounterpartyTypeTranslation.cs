using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("CounterpartyTypeId", "LanguageId")]
[Table("cmn_counterparty_type_translation")]
[Index("LanguageId", Name = "ix_cmn_counterparty_type_translation_language_id")]
public partial class CmnCounterpartyTypeTranslation
{
    [Key]
    [Column("counterparty_type_id")]
    public short CounterpartyTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("CounterpartyTypeId")]
    [InverseProperty("CmnCounterpartyTypeTranslations")]
    public virtual CmnCounterpartyType CounterpartyType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnCounterpartyTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
