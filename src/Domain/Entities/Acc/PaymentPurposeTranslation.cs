using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}
