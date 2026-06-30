using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("PaymentPurposeId", "LanguageId")]
[Table("acc_payment_purpose_translation")]
public partial class PaymentPurposeTranslation
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
    [InverseProperty("PaymentPurposeTranslations")]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PaymentPurposeId")]
    [InverseProperty("PaymentPurposeTranslations")]
    public virtual PaymentPurpose PaymentPurpose { get; set; } = null!;
}
