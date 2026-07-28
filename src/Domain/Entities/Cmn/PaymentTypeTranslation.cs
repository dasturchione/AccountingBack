using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("PaymentTypeId", "LanguageId")]
[Table("cmn_payment_type_translation")]
public partial class PaymentTypeTranslation
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
    [InverseProperty(nameof(Entities.Language.PaymentTypeTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("PaymentTypeId")]
    [InverseProperty(nameof(Entities.PaymentType.PaymentTypeTranslations))]
    public virtual PaymentType PaymentType { get; set; } = null!;
}
