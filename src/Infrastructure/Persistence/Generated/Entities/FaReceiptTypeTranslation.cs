using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Generated.Entities;

[PrimaryKey("ReceiptTypeId", "LanguageId")]
[Table("fa_receipt_type_translation")]
[Index("LanguageId", Name = "ix_fa_receipt_type_translation_language_id")]
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
    [InverseProperty("FaReceiptTypeTranslations")]
    public virtual CmnLanguage Language { get; set; } = null!;

    [ForeignKey("ReceiptTypeId")]
    [InverseProperty("FaReceiptTypeTranslations")]
    public virtual FaReceiptType ReceiptType { get; set; } = null!;
}
