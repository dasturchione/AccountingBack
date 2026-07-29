using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PaymentTypeId", "LanguageId")]
[Table("cmn_payment_type_translation")]
[Index("LanguageId", Name = "ix_cmn_payment_type_translation_language_id")]
public partial class CmnPaymentTypeTranslation
{
    [Key]
    [Column("payment_type_id")]
    public short PaymentTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("CmnPaymentTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty("CmnPaymentTypeTranslations")]
    public virtual CmnPaymentType PaymentType { get; set; } = null!;
}
