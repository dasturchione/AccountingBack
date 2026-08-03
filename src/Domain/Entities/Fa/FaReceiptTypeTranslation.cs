using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[PrimaryKey("ReceiptTypeId", "LanguageId")]
[Table("fa_receipt_type_translation")]
public partial class FaReceiptTypeTranslation
{
    [Key]
    [Column("receipt_type_id")]
    public short ReceiptTypeId { get; set; }

    [Key]
    [Column("language_id")]
    public short LanguageId { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [ForeignKey("LanguageId")]
    [InverseProperty(nameof(Language.FaReceiptTypeTranslations))]
    public virtual Language Language { get; set; } = null!;

    [ForeignKey("ReceiptTypeId")]
    [InverseProperty(nameof(FaReceiptType.FaReceiptTypeTranslations))]
    public virtual FaReceiptType ReceiptType { get; set; } = null!;
}
