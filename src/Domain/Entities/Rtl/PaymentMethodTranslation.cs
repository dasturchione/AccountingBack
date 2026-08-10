using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("PaymentMethodId", "LanguageId")]
[Table("rtl_payment_method_translation")]
public partial class PaymentMethodTranslation
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

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Language.PaymentMethodTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey(nameof(PaymentMethodId))]
    [InverseProperty(nameof(PaymentMethod.PaymentMethodTranslations))]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
