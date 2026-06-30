using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("PaymentPurposeId", "LanguageId")]
[Table("acc_payment_purpose_translation")]
public partial class AccPaymentPurposeTranslation
{
    [Key]
    [Column("payment_purpose_id")]
    public short PaymentPurposeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty("AccPaymentPurposeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("PaymentPurposeId")]
    [InverseProperty("AccPaymentPurposeTranslations")]
    public virtual AccPaymentPurpose PaymentPurpose { get; set; } = null!;
}
