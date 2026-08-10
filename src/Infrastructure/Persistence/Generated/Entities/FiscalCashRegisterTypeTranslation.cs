using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("CashRegisterTypeId", "LanguageId")]
[Table("fiscal_cash_register_type_translation")]
[Index("LanguageId", Name = "ix_fiscal_cash_register_type_translation_language_id")]
public partial class FiscalCashRegisterTypeTranslation
{
    [Key]
    [Column("cash_register_type_id")]
    public short CashRegisterTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey("CashRegisterTypeId")]
    [InverseProperty("FiscalCashRegisterTypeTranslations")]
    public virtual FiscalCashRegisterType CashRegisterType { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("FiscalCashRegisterTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;
}
