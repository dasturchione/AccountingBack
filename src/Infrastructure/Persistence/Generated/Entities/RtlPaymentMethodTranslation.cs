using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PaymentMethodId", "LanguageId")]
[Table("rtl_payment_method_translation")]
public partial class RtlPaymentMethodTranslation
{
    [Key]
    [Column("payment_method_id")]
    public short PaymentMethodId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("RtlPaymentMethodTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("RtlPaymentMethodTranslations")]
    public virtual RtlPaymentMethod PaymentMethod { get; set; } = null!;
}
