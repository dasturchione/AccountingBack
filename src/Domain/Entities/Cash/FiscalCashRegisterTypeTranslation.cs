using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("CashRegisterTypeId", "LanguageId")]
[Table("fiscal_cash_register_type_translation")]
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

    [ForeignKey(nameof(CashRegisterTypeId))]
    [InverseProperty(nameof(FiscalCashRegisterType.FiscalCashRegisterTypeTranslations))]
    public virtual FiscalCashRegisterType CashRegisterType { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Language.FiscalCashRegisterTypeTranslations))]
    public virtual Language Language { get; set; } = null!;
}
