using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[PrimaryKey(nameof(PaymentAcceptancePointTypeId), nameof(LanguageId))]
[Table("cmn_payment_acceptance_point_type_translation")]
[Index(nameof(LanguageId), Name = "ix_cmn_payment_acceptance_point_type_translation_language_id")]
public sealed class PaymentAcceptancePointTypeTranslation
{
    [Key]
    [Column("payment_acceptance_point_type_id")]
    public short PaymentAcceptancePointTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [ForeignKey(nameof(PaymentAcceptancePointTypeId))]
    [InverseProperty(nameof(Entities.PaymentAcceptancePointType.Translations))]
    public PaymentAcceptancePointType PaymentAcceptancePointType { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    [InverseProperty(nameof(Entities.Language.PaymentAcceptancePointTypeTranslations))]
    public Language Language { get; set; } = null!;
}
